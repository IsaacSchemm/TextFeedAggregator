using Pleronet;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace TextFeedAggregator.Backend {
    public class MastodonSource : ISource {
        private readonly MastodonClient _client;

        public MastodonSource(MastodonClient client) {
            _client = client;
        }

        public IEnumerable<string> SourceIdentifiers => new[] { _client.AppRegistration.Instance };

        public async IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync() {
            string max_id = "";
            while (true) {
                var statuses = await _client.GetHomeTimeline(max_id);
                if (!statuses.Any())
                    break;
 
                foreach (var s in statuses) {
                    var author = new Author {
                        Username = s.Account.UserName,
                        AvatarUrl = s.Account.AvatarUrl,
                        ProfileUrl = s.Account.ProfileUrl
                    };
                    yield return new StatusUpdate {
                        SourceIdentifier = _client.AppRegistration.Instance,
                        Id = s.Id,
                        Author = author,
                        Timestamp = s.CreatedAt,
                        LinkUrl = s.Url,
                        Html = !string.IsNullOrEmpty(s.SpoilerText)
                            ? WebUtility.HtmlEncode(s.SpoilerText)
                            : s.Content,
                        RepostedFrom = s.Reblog?.Account?.UserName
                    };
                }
                max_id = statuses.NextPageMaxId;
            }
        }
    }
}
