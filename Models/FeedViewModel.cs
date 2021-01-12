using System;
using System.Collections.Immutable;

namespace TextFeedAggregator.Models {
    public record Author {
        public string Username { get; init; }
        public string AvatarUrl { get; init; }
    }

    public record FeedItem {
        public string LinkUrl { get; init; }
        public DateTimeOffset Timestamp { get; init; }
        public Author Author { get; init; }
        public string Html { get; init; }
    }

    public record FeedViewModel {
        public Guid Key { get; init; }
        public bool HasMore { get; init; }
        public int? NextOffset { get; init; }
        public bool HasLess { get; init; }
        public int? LastOffset { get; init; }
        public ImmutableList<FeedItem> FeedItems { get; init; }
        public DateTimeOffset Latest { get; init; }
    }
}
