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

        public IEnumerable<string> Hosts => new[] { "deviantart.com" };

        public async IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync() {
            await foreach (var p in DeviantArtFs.Api.Browse.AsyncGetPostsByDeviantsYouWatch(_token, 0).ToAsyncEnumerable()) {
                if (p.status.OrNull() is DeviantArtStatus s) {
                    yield return new StatusUpdate {
                        Host = "deviantart.com",
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

        public async Task PostStatusUpdateAsync(string host, string text) {
            if (!Hosts.Contains(host))
                throw new ArgumentException("Given host is not supported by this source", nameof(host));

            await DeviantArtFs.Api.User.AsyncPostStatus(_token, new DeviantArtFs.Api.User.StatusPostRequest(text)).StartAsTask();
        }
    }
}
