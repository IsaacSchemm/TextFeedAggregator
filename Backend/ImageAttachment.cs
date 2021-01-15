using System;

namespace TextFeedAggregator.Backend {
    public record ImageAttachment {
        public byte[] Data { get; init; }
        public string ContentType { get; init; }
        public string Description { get; init; }

        public string GenerateFilename() => Guid.NewGuid() + "." + ContentType;
    }
}
