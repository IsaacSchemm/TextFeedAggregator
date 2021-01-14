using System.Collections.Generic;
using System.Threading.Tasks;

namespace TextFeedAggregator.Backend {
    public interface ISource {
        IEnumerable<string> SourceIdentifiers { get; }

        IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync();
    }
}
