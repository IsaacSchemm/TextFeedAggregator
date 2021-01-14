namespace TextFeedAggregator.Backend {
    public record NotificationSummary {
        public string Host { get; init; }
        public int Count { get; init; }
        public bool PossiblyMore { get; init; }
        public string Url { get; init; }
    }
}
