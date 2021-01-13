using System.Collections.Generic;
using System.Threading.Tasks;

namespace TextFeedAggregator.Backend {
    public interface ISource {
        string SourceIdentifier { get; }
        string NotificationsUrl { get; }

        Task<Author> GetAuthenticatedUserAsync();
        IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync();
    }
}
