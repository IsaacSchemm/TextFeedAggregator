namespace TextFeedAggregator.Backend {
    public record Author {
        public string Username { get; init; }
        public string AvatarUrl { get; init; }
        public string ProfileUrl { get; init; }
    }
}
