using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TextFeedAggregator.Backend {
    public class CompositeSource : ISource {
        public IEnumerable<string> Hosts => _sources
            .SelectMany(x => x.Hosts)
            .Distinct();

        private readonly IReadOnlyList<ISource> _sources;

        public CompositeSource(IEnumerable<ISource> sources) {
            _sources = sources.ToList();
        }

        public async IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync() {
            var enumerators = _sources
                .Select(x => x.GetStatusUpdatesAsync().GetAsyncEnumerator())
                .ToList();
            foreach (var e in enumerators.ToList()) {
                bool result = await e.MoveNextAsync();
                if (!result)
                    enumerators.Remove(e);
            }
            while (enumerators.Any()) {
                var most_recent = enumerators.OrderByDescending(x => x.Current.Timestamp).First();
                yield return most_recent.Current;
                if (!await most_recent.MoveNextAsync())
                    enumerators.Remove(most_recent);
            }
        }

        public async Task<IEnumerable<NotificationSummary>> GetNotificationSummariesAsync() {
            var lists = await Task.WhenAll(_sources.Select(x => x.GetNotificationSummariesAsync()));
            return lists.SelectMany(x => x);
        }

        public async Task PostStatusUpdateAsync(IEnumerable<string> hosts, string text, ImageAttachment image = null) {
            await Task.WhenAll(_sources.Select(x => x.PostStatusUpdateAsync(hosts, text, image)));
        }

        public async Task DeleteStatusUpdateAsync(string host, string id) {
            await Task.WhenAll(_sources.Select(x => x.DeleteStatusUpdateAsync(host, id)));
        }
    }
}
