using System;

namespace TextFeedAggregator.Backend {
    public record StatusUpdate {
        public string SourceIdentifier { get; init; }
        public string Id { get; init; }
        public Author Author { get; init; }
        public DateTimeOffset Timestamp { get; init; }
        public string RepostedFrom { get; init; }
        public string Html { get; init; }
        public string LinkUrl { get; init; }
    }
}
