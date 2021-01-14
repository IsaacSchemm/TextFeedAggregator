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

        public string Host => "deviantart.com";

        public IEnumerable<string> Hosts => new[] { Host };

        public async IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync() {
            await foreach (var p in DeviantArtFs.Api.Browse.AsyncGetPostsByDeviantsYouWatch(_token, 0).ToAsyncEnumerable()) {
                if (p.status.OrNull() is DeviantArtStatus s) {
                    yield return new StatusUpdate {
                        Host = Host,
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

        public async Task<IEnumerable<NotificationSummary>> GetNotificationSummariesAsync() {
            var messages = await DeviantArtFs.Api.Messages.AsyncGetFeed(
                _token,
                new DeviantArtFs.Api.Messages.MessagesFeedRequest { Stack = false },
                Microsoft.FSharp.Core.FSharpOption<string>.None).Take(50).ThenToList().StartAsTask();
            return new[] {
                new NotificationSummary {
                    Host = Host,
                    Count = messages.Length,
                    PossiblyMore = messages.Length >= 50,
                    Url = "https://www.deviantart.com/notifications/feedback"
                }
            };
        }

        public async Task PostStatusUpdateAsync(IEnumerable<string> hosts, string text) {
            if (hosts.Contains(Host))
                await DeviantArtFs.Api.User.AsyncPostStatus(_token, new DeviantArtFs.Api.User.StatusPostRequest(text)).StartAsTask();
        }
    }
}
