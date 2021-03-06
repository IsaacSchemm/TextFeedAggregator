﻿using DeviantArtFs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Pleronet;
using Pleronet.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TextFeedAggregator.Backend;
using TextFeedAggregator.Data;
using TextFeedAggregator.Models;
using Tweetinvi;
using Tweetinvi.Models;

namespace TextFeedAggregator.Controllers {
    public class HomeController : Controller {
        public record ControllerCacheItem {
            public Guid Id { get; init; }
            public string LocalUserId { get; init; }
            public IReadOnlyList<string> Hosts { get; init; }
            public IReadOnlyList<NotificationSummary> NotificationSummaries { get; init; }
            public AsyncEnumerableCache<StatusUpdate> StatusUpdates { get; init; }
        }

        private readonly ApplicationDbContext _context;
        private readonly DeviantArtApp _deviantArtApp;
        private readonly IReadOnlyConsumerCredentials _twitterCredentials;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMemoryCache _cache; 

        public HomeController(ApplicationDbContext context, DeviantArtApp deviantArtApp, IReadOnlyConsumerCredentials twitterCredentials, UserManager<IdentityUser> userManager, IMemoryCache cache) {
            _context = context;
            _deviantArtApp = deviantArtApp;
            _twitterCredentials = twitterCredentials;
            _userManager = userManager;
            _cache = cache;
        }

        private async IAsyncEnumerable<ISource> CollectSourcesAsync() {
            string userId = _userManager.GetUserId(User);
            var da = await _context.UserDeviantArtTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .FirstOrDefaultAsync();
            if (da != null) {
                var w = new DeviantArtTokenWrapper(_deviantArtApp, _context, da);
                yield return new DeviantArtSource(w);
            }
            var twitter = await _context.UserTwitterTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .FirstOrDefaultAsync();
            if (twitter != null) {
                var client = new TwitterClient(
                    _twitterCredentials.ConsumerKey,
                    _twitterCredentials.ConsumerSecret,
                    twitter.AccessToken,
                    twitter.AccessTokenSecret);
                yield return new TwitterSource(client);
            }
            var mastodon = await _context.UserMastodonTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .ToListAsync();
            foreach (var m in mastodon) {
                var client = new MastodonClient(
                    new AppRegistration { Instance = m.Host },
                    new Auth { AccessToken = m.AccessToken });
                yield return new MastodonSource(client);
            }
        }

        private async Task<ISource> GetCompositeSourceAsync() {
            return new CompositeSource(await CollectSourcesAsync().ToListAsync());
        }

        public async Task<IActionResult> Index(int offset = 0, int limit = 25, DateTimeOffset? latest = null, Guid? key = null) {
            string userId = _userManager.GetUserId(User);

            ControllerCacheItem cacheItem;
            if (key != null && _cache.TryGetValue(key, out object o) && o is ControllerCacheItem c && c.LocalUserId == userId) {
                cacheItem = c;
            } else {
                ISource source = await GetCompositeSourceAsync();
                cacheItem = new ControllerCacheItem {
                    Id = Guid.NewGuid(),
                    LocalUserId = userId,
                    Hosts = source.Hosts.OrderBy(x => x).ToList(),
                    NotificationSummaries = (await source.GetNotificationSummariesAsync()).OrderBy(x => x.Host).ToList(),
                    StatusUpdates = new AsyncEnumerableCache<StatusUpdate>(source.GetStatusUpdatesAsync())
                };
                _cache.Set(cacheItem.Id, cacheItem, DateTimeOffset.UtcNow.AddMinutes(15));
            }

            var page = await cacheItem.StatusUpdates.Skip(offset).Take(limit).ToListAsync();
            bool hasMore = await cacheItem.StatusUpdates.Skip(offset + limit).AnyAsync();

            return View(new FeedViewModel {
                Key = cacheItem.Id,
                Hosts = cacheItem.Hosts,
                NotificationSummaries = cacheItem.NotificationSummaries,
                Latest = latest ?? page.Select(x => x.Timestamp).DefaultIfEmpty(DateTimeOffset.MinValue).First(),
                StatusUpdates = page,
                LastOffset = Math.Max(offset - limit, 0),
                HasLess = offset > 0,
                NextOffset = offset + limit,
                HasMore = hasMore
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PostStatus(IFormFileCollection file, string[] host, string text, string altText) {
            ISource source = await GetCompositeSourceAsync();
            switch (file.Count) {
                case 0:
                    await source.PostStatusUpdateAsync(host, text);
                    break;
                case 1:
                    var f = file.Single();
                    using (var ms = new MemoryStream()) {
                        await f.OpenReadStream().CopyToAsync(ms);
                        await source.PostStatusUpdateAsync(host, text, new ImageAttachment {
                            ContentType = file.Single().ContentType,
                            Data = ms.ToArray(),
                            Description = string.IsNullOrWhiteSpace(altText)
                                ? null
                                : altText
                        });
                    }
                    break;
                default:
                    return Content("Only one image can be uploaded.");
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStatus(string host, string id) {
            ISource source = await GetCompositeSourceAsync();
            await source.DeleteStatusUpdateAsync(host, id);
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
