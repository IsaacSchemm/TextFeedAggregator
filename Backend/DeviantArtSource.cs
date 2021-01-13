using DeviantArtFs;
using DeviantArtFs.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TextFeedAggregator.Backend {
    public class DeviantArtSource : ISource {
        private readonly IDeviantArtAccessToken _token;

        public DeviantArtSource(IDeviantArtAccessToken token) {
            _token = token;
        }

        public string SourceIdentifier => "deviantart";
        public string NotificationsUrl => "https://www.deviantart.com/notifications/feedback";

        public async Task<Author> GetAuthenticatedUserAsync() {
            var user = await DeviantArtFs.Api.User.AsyncWhoami(_token, DeviantArtObjectExpansion.None).StartAsTask();
            return new Author {
                Username = user.username,
                AvatarUrl = user.usericon,
                ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(user.username)}"
            };
        }

        public async IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync() {
            await foreach (var p in DeviantArtFs.Api.Browse.AsyncGetPostsByDeviantsYouWatch(_token, 0).ToAsyncEnumerable()) {
                if (p.status.OrNull() is DeviantArtStatus s) {
                    yield return new StatusUpdate {
                        SourceIdentifier = this.SourceIdentifier,
                        Id = s.statusid.OrNull()?.ToString(),
                        Author = s.author.OrNull() is DeviantArtUser a
                            ? new Author {
                                Username = a.username,
                                AvatarUrl = a.usericon,
                                ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(a.username)}"
                            } : new Author {
                                Username = "???",
                                AvatarUrl = null,
                                ProfileUrl = null
                            },
                        Timestamp = s.ts.OrNull() ?? DateTimeOffset.UtcNow,
                        RepostedFrom = null,
                        Html = s.body.OrNull() ?? "",
                        LinkUrl = s.url.OrNull() ?? "",
                    };
                }
            }
        }
    }
}
