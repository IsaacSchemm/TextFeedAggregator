using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TextFeedAggregator.Backend;
using TextFeedAggregator.Models;

namespace TextFeedAggregator.Controllers {
    public class HomeController : Controller {
        public record ControllerCacheItem {
            public Guid Id { get; init; }
            public string LocalUserId { get; init; }
            public Author RemoteUser { get; init; }
            public AsyncEnumerableCache<StatusUpdate> StatusUpdates { get; init; }
            public string NotificationsUrl { get; init; }
        }

        protected UserManager<IdentityUser> _userManager;
        protected IMemoryCache _cache; 

        public HomeController(UserManager<IdentityUser> userManager, IMemoryCache cache) {
            _userManager = userManager;
            _cache = cache;
        }

        public async Task<IActionResult> Index(int offset = 0, int limit = 25, DateTimeOffset? latest = null, Guid? key = null) {
            string userId = _userManager.GetUserId(User);

            ControllerCacheItem cacheItem;
            if (key != null && _cache.TryGetValue(key, out object o) && o is ControllerCacheItem c && c.LocalUserId == userId) {
                cacheItem = c;
            } else {
                var source = new EmptySource();
                cacheItem = new ControllerCacheItem {
                    Id = Guid.NewGuid(),
                    LocalUserId = userId,
                    RemoteUser = await source.GetAuthenticatedUserAsync(),
                    StatusUpdates = new AsyncEnumerableCache<StatusUpdate>(source.GetStatusUpdatesAsync()),
                    NotificationsUrl = source.NotificationsUrl
                };
                _cache.Set(cacheItem.Id, cacheItem, DateTimeOffset.UtcNow.AddMinutes(15));
            }

            var page = await cacheItem.StatusUpdates.Skip(offset).Take(limit).ToListAsync();
            bool hasMore = await cacheItem.StatusUpdates.Skip(offset + limit).AnyAsync();

            return View(new FeedViewModel {
                Key = cacheItem.Id,
                Latest = latest ?? page.Select(x => x.Timestamp).DefaultIfEmpty(DateTimeOffset.MinValue).First(),
                StatusUpdates = page,
                LastOffset = Math.Max(offset - limit, 0),
                HasLess = offset > 0,
                NextOffset = offset + limit,
                HasMore = hasMore
            });
        }

        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
