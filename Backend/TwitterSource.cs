using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace TextFeedAggregator.Backend {
    public class TwitterSource : ISource {
        private readonly TwitterClient _client;

        public TwitterSource(TwitterClient client) {
            _client = client;
        }

        public string Host => "twitter.com";

        public IEnumerable<string> Hosts => new[] { Host };

        public async IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync() {
            var self = await _client.Users.GetAuthenticatedUserAsync();

            var parameters = new GetHomeTimelineParameters {
                PageSize = 100,
                IncludeEntities = true
            };
            while (true) {
                ITweet[] page = await _client.Timelines.GetHomeTimelineAsync(parameters);
                if (!page.Any())
                    break;

                foreach (var t in page) {
                    yield return new StatusUpdate {
                        Host = Host,
                        Id = t.IdStr,
                        CanDelete = t.CreatedBy.Id == self.Id,
                        Author = new Author {
                            Username = t.CreatedBy.ScreenName,
                            AvatarUrl = t.CreatedBy.ProfileImageUrl,
                            ProfileUrl = $"https://twitter.com/{Uri.EscapeDataString(t.CreatedBy.ScreenName)}"
                        },
                        Timestamp = t.CreatedAt,
                        LinkUrl = t.RetweetedTweet?.Url ?? t.Url,
                        Html = WebUtility.HtmlEncode(WebUtility.HtmlDecode(t.FullText ?? t.Text)),
                        AdditionalImages = t.Entities.Medias.Where(m => m.MediaType == "photo").Select(m => new StatusUpdateMedia {
                            ImageUrl = m.MediaURLHttps,
                            LinkUrl = m.URL
                        }).ToList(),
                        RepostedFrom = t.RetweetedTweet?.CreatedBy?.ScreenName
                    };
                }
                parameters.MaxId = page.Select(x => x.Id).Min() - 1;
            }
        }

        public Task<IEnumerable<NotificationSummary>> GetNotificationSummariesAsync() {
            IEnumerable<NotificationSummary> arr = new[] {
                new NotificationSummary {
                    Host = Host,
                    Count = 0,
                    PossiblyMore = true,
                    Url = "https://twitter.com/notifications"
                }
            };
            return Task.FromResult(arr);
        }

        private async IAsyncEnumerable<long> UploadMediaAsync(IEnumerable<ImageAttachment> images) {
            foreach (var i in images) {
                var media = await _client.Upload.UploadTweetImageAsync(i.Data);
                if (media.Id is long l)
                    yield return l;
            }
        }

        public async Task PostStatusUpdateAsync(IEnumerable<string> hosts, string text, IEnumerable<ImageAttachment> images) {
            if (hosts.Contains(Host))
                await _client.Tweets.PublishTweetAsync(new PublishTweetParameters {
                    Text = text,
                    MediaIds = await UploadMediaAsync(images).ToListAsync()
                });
        }

        public async Task PostStatusUpdateAsync(IEnumerable<string> hosts, string text, ImageAttachment image = null) {
            await PostStatusUpdateAsync(hosts, text, new[] { image }.Where(x => x != null));
        }

        public async Task DeleteStatusUpdateAsync(string host, string id) {
            if (host == Host)
                await _client.Tweets.DestroyTweetAsync(long.Parse(id));
        }
    }
}
