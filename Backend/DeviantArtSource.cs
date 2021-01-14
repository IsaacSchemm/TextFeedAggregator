using DeviantArtFs;
using DeviantArtFs.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TextFeedAggregator.Backend {
    public class DeviantArtSource : ISource {
        private readonly IDeviantArtAccessToken _token;

        public DeviantArtSource(IDeviantArtAccessToken token) {
            _token = token;
        }

        public IEnumerable<string> SourceIdentifiers => new[] { "deviantart" };

        public async IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync() {
            await foreach (var p in DeviantArtFs.Api.Browse.AsyncGetPostsByDeviantsYouWatch(_token, 0).ToAsyncEnumerable()) {
                if (p.status.OrNull() is DeviantArtStatus s) {
                    yield return new StatusUpdate {
                        SourceIdentifier = "deviantart",
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
