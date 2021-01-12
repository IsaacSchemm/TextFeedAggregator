using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TextFeedAggregator.Models;

namespace TextFeedAggregator.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) {
            _logger = logger;
        }

        public IActionResult Index() {
            return View(new FeedViewModel {
                FeedItems = System.Collections.Immutable.ImmutableList<FeedItem>.Empty,
                HasLess = false,
                HasMore = false,
                NextOffset = null,
                Key = Guid.NewGuid(),
                LastOffset = null,
                Latest = DateTimeOffset.UtcNow
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
