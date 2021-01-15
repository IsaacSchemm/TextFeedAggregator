using Pleronet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TextFeedAggregator.Backend {
    public class MastodonSource : ISource {
        private readonly MastodonClient _client;

        public MastodonSource(MastodonClient client) {
            _client = client;
        }

        public string Host => _client.AppRegistration.Instance;

        public IEnumerable<string> Hosts => new[] { Host };

        public async IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync() {
            var self = await _client.GetCurrentUser();

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
                        Host = Host,
                        Id = s.Id,
                        CanDelete = s.Account.Id == self.Id,
                        Author = author,
                        Timestamp = s.CreatedAt,
                        LinkUrl = s.Url,
                        Html = !string.IsNullOrEmpty(s.SpoilerText)
                            ? WebUtility.HtmlEncode(s.SpoilerText)
                            : s.Content,
                        AdditionalImages = s.MediaAttachments.Select(m => new StatusUpdateMedia {
                            ImageUrl = m.Url,
                            LinkUrl = m.Url,
                            AltText = m.Description
                        }).ToList(),
                        RepostedFrom = s.Reblog?.Account?.UserName
                    };
                }
                max_id = statuses.NextPageMaxId;
            }
        }

        private async IAsyncEnumerable<Pleronet.Entities.Notification> GetNotificationsAsync() {
            string max_id = "";
            while (true) {
                var notifications = await _client.GetNotifications(max_id);
                if (!notifications.Any())
                    break;

                foreach (var n in notifications)
                    yield return n;

                max_id = notifications.NextPageMaxId;
            }
        }

        public async Task<IEnumerable<NotificationSummary>> GetNotificationSummariesAsync() {
            var notifications = await GetNotificationsAsync().Take(50).ToListAsync();
            return new[] {
                new NotificationSummary {
                    Host = Host,
                    Count = notifications.Count,
                    PossiblyMore = notifications.Count >= 50,
                    Url = $"https://{Host}"
                }
            };
        }

        private async IAsyncEnumerable<string> UploadMediaAsync(IEnumerable<ImageAttachment> images) {
            foreach (var i in images) {
                using var ms = new MemoryStream(i.Data);
                var media = await _client.UploadMedia(new MediaDefinition(ms, i.GenerateFilename()), description: i.Description);
                yield return media.Id;
            }
        }

        public async Task PostStatusUpdateAsync(IEnumerable<string> hosts, string text, IEnumerable<ImageAttachment> images) {
            if (hosts.Contains(Host))
                await _client.PostStatus(text, mediaIds: await UploadMediaAsync(images).ToListAsync());
        }

        public async Task PostStatusUpdateAsync(IEnumerable<string> hosts, string text, ImageAttachment image = null) {
            await PostStatusUpdateAsync(hosts, text, new[] { image }.Where(x => x != null));
        }

        public async Task DeleteStatusUpdateAsync(string host, string id) {
            if (host == Host)
                await _client.DeleteStatus(id);
        }
    }
}
