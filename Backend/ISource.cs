using System.Collections.Generic;
using System.Threading.Tasks;

namespace TextFeedAggregator.Backend {
    public interface ISource {
        IEnumerable<string> Hosts { get; }
        IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync();
        Task<IEnumerable<NotificationSummary>> GetNotificationSummariesAsync();
        Task PostStatusUpdateAsync(IEnumerable<string> hosts, string text, ImageAttachment image = null);
        Task DeleteStatusUpdateAsync(string host, string id);
    }
}
