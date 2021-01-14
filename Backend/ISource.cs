using System.Collections.Generic;
using System.Threading.Tasks;

namespace TextFeedAggregator.Backend {
    public interface ISource {
        IEnumerable<string> Hosts { get; }
        IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync();
        Task PostStatusUpdateAsync(string host, string text);
    }
}
