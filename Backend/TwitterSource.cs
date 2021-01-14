using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace TextFeedAggregator.Backend {
    public class TwitterSource : ISource {
        private readonly TwitterClient _client;

        public TwitterSource(TwitterClient client) {
            _client = client;
        }

        public string Host => "twitter.com";

        public IEnumerable<string> Hosts => new[] { Host };

        public async IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync() {
            var parameters = new Tweetinvi.Parameters.GetHomeTimelineParameters {
                PageSize = 100
            };
            while (true) {
                ITweet[] page = await _client.Timelines.GetHomeTimelineAsync(parameters);
                if (!page.Any())
                    break;

                foreach (var t in page) {
                    yield return new StatusUpdate {
                        Host = Host,
                        Id = t.IdStr,
                        Author = new Author {
                            Username = $"@{t.CreatedBy.ScreenName}",
                            AvatarUrl = t.CreatedBy.ProfileImageUrl,
                            ProfileUrl = $"https://twitter.com/{Uri.EscapeDataString(t.CreatedBy.ScreenName)}"
                        },
                        Timestamp = t.CreatedAt,
                        LinkUrl = t.RetweetedTweet?.Url ?? t.Url,
                        Html = WebUtility.HtmlEncode(WebUtility.HtmlDecode(t.FullText ?? t.Text)),
                        RepostedFrom = t.RetweetedTweet?.CreatedBy?.ScreenName
                    };
                }
                parameters.MaxId = page.Select(x => x.Id).Min() - 1;
            }
        }

        public async Task PostStatusUpdateAsync(IEnumerable<string> hosts, string text) {
            if (hosts.Contains(Host))
                await _client.Tweets.PublishTweetAsync(text);
        }
    }
}
