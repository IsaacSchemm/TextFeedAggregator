﻿using Pleronet;
using System;
using System.Collections.Generic;
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

        public async Task PostStatusUpdateAsync(IEnumerable<string> hosts, string text) {
            if (hosts.Contains(Host))
                await _client.PostStatus(text);
        }
    }
}
