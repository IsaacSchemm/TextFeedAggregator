using System;
using System.Collections.Generic;

namespace TextFeedAggregator.Backend {
    public record StatusUpdate {
        public string Host { get; init; }
        public string Id { get; init; }
        public bool CanDelete { get; init; }
        public Author Author { get; init; } = new Author();
        public DateTimeOffset Timestamp { get; init; }
        public string RepostedFrom { get; init; }
        public string Html { get; init; }
        public IReadOnlyList<StatusUpdateMedia> AdditionalImages { get; init; } = Array.Empty<StatusUpdateMedia>();
        public string LinkUrl { get; init; }
    }
}
