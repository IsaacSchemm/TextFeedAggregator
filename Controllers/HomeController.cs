﻿using DeviantArtFs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Pleronet;
using Pleronet.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TextFeedAggregator.Backend;
using TextFeedAggregator.Data;
using TextFeedAggregator.Models;

namespace TextFeedAggregator.Controllers {
    public class HomeController : Controller {
        public record ControllerCacheItem {
            public Guid Id { get; init; }
            public string LocalUserId { get; init; }
            public IReadOnlyList<string> Hosts { get; init; }
            public AsyncEnumerableCache<StatusUpdate> StatusUpdates { get; init; }
        }

        protected ApplicationDbContext _context;
        protected DeviantArtApp _deviantArtApp;
        protected UserManager<IdentityUser> _userManager;
        protected IMemoryCache _cache; 

        public HomeController(ApplicationDbContext context, DeviantArtApp deviantArtApp, UserManager<IdentityUser> userManager, IMemoryCache cache) {
            _context = context;
            _deviantArtApp = deviantArtApp;
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
                    StatusUpdates = new AsyncEnumerableCache<StatusUpdate>(source.GetStatusUpdatesAsync())
                };
                _cache.Set(cacheItem.Id, cacheItem, DateTimeOffset.UtcNow.AddMinutes(15));
            }

            var page = await cacheItem.StatusUpdates.Skip(offset).Take(limit).ToListAsync();
            bool hasMore = await cacheItem.StatusUpdates.Skip(offset + limit).AnyAsync();

            return View(new FeedViewModel {
                Key = cacheItem.Id,
                Hosts = cacheItem.Hosts,
                Latest = latest ?? page.Select(x => x.Timestamp).DefaultIfEmpty(DateTimeOffset.MinValue).First(),
                StatusUpdates = page,
                LastOffset = Math.Max(offset - limit, 0),
                HasLess = offset > 0,
                NextOffset = offset + limit,
                HasMore = hasMore
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PostStatus(IEnumerable<string> host, string text) {
            ISource source = await GetCompositeSourceAsync();
            await source.PostStatusUpdateAsync(host, text);
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
