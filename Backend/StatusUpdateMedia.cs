namespace TextFeedAggregator.Backend {
    public record StatusUpdateMedia {
        public string ImageUrl { get; init; }
        public string LinkUrl { get; init; }
        public string AltText { get; init; }
    }
}
