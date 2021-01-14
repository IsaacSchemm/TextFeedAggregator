using System.Collections.Generic;
using System.Linq;

namespace TextFeedAggregator.Backend {
    public class EmptySource : ISource {
        public IEnumerable<string> SourceIdentifiers => Enumerable.Empty<string>();

        public IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync() {
            return AsyncEnumerable.Empty<StatusUpdate>();
        }
    }
}
