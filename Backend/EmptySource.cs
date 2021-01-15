using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TextFeedAggregator.Backend {
    public class EmptySource : ISource {
        public IEnumerable<string> Hosts => Enumerable.Empty<string>();

        public IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync() {
            return AsyncEnumerable.Empty<StatusUpdate>();
        }

        public Task<IEnumerable<NotificationSummary>> GetNotificationSummariesAsync() => Task.FromResult(Enumerable.Empty<NotificationSummary>());

        public Task PostStatusUpdateAsync(IEnumerable<string> hosts, string text) => Task.CompletedTask;

        public Task DeleteStatusUpdateAsync(string host, string id) => Task.CompletedTask;
    }
}
