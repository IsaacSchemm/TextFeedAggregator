using DeviantArtFs;
using DeviantArtFs.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
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
                        CanDelete = false,
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

        private async Task<long> UploadMediaAsync(ImageAttachment image) {
            using var ms = new MemoryStream(image.Data);
            var stashItem = await DeviantArtFs.Api.Stash.AsyncSubmit(
                _token,
                new DeviantArtFs.Api.Stash.SubmitRequest(image.GenerateFilename(), image.ContentType, image.Data) {
                    ArtistComments = image.Description + " (Uploaded from Text Feed Aggregator)",
                    Title = $"TFA-{DateTimeOffset.UtcNow:o}"
                }).StartAsTask();
            return stashItem.itemid;
        }

        public async Task PostStatusUpdateAsync(IEnumerable<string> hosts, string text, ImageAttachment image = null) {
            if (hosts.Contains(Host))
                await DeviantArtFs.Api.User.AsyncPostStatus(_token, new DeviantArtFs.Api.User.StatusPostRequest(text) {
                    Stashid = image == null
                        ? (long?)null
                        : await UploadMediaAsync(image)
                }).StartAsTask();
        }

        public Task DeleteStatusUpdateAsync(string host, string id) {
            return Task.CompletedTask;
        }
    }
}
