using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TextFeedAggregator.Backend {
    public class EmptySource : ISource {
        public string SourceIdentifier => "Empty";

        public string NotificationsUrl => "https://www.example.org";

        public Task DeleteStatusUpdateAsync(string id) {
            throw new NotImplementedException();
        }

        public Task<Author> GetAuthenticatedUserAsync() {
            return Task.FromResult(new Author {
                Username = "None"
            });
        }

        public IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync() {
            return AsyncEnumerable.Empty<StatusUpdate>();
        }
    }
}
