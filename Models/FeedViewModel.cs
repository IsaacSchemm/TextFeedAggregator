using System;
using System.Collections.Generic;
using TextFeedAggregator.Backend;

namespace TextFeedAggregator.Models {
    public record FeedViewModel {
        public Guid Key { get; init; }
        public IReadOnlyList<string> Hosts { get; init; }
        public bool HasMore { get; init; }
        public int? NextOffset { get; init; }
        public bool HasLess { get; init; }
        public int? LastOffset { get; init; }
        public IReadOnlyList<NotificationSummary> NotificationSummaries { get; init; }
        public IReadOnlyList<StatusUpdate> StatusUpdates { get; init; }
        public DateTimeOffset Latest { get; init; }
    }
}
