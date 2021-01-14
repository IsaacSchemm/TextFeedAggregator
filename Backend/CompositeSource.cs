using System.Collections.Generic;
using System.Linq;

namespace TextFeedAggregator.Backend {
    public class CompositeSource : ISource {
        public IEnumerable<string> SourceIdentifiers => _sources
            .SelectMany(x => x.SourceIdentifiers)
            .Distinct();

        private readonly IReadOnlyList<ISource> _sources;

        public CompositeSource(IEnumerable<ISource> sources) {
            _sources = sources.ToList();
        }

        public async IAsyncEnumerable<StatusUpdate> GetStatusUpdatesAsync() {
            var enumerators = _sources
                .Select(x => x.GetStatusUpdatesAsync().GetAsyncEnumerator())
                .ToList();
            foreach (var e in enumerators.ToList()) {
                bool result = await e.MoveNextAsync();
                if (!result)
                    enumerators.Remove(e);
            }
            while (enumerators.Any()) {
                var most_recent = enumerators.OrderByDescending(x => x.Current.Timestamp).First();
                yield return most_recent.Current;
                if (!await most_recent.MoveNextAsync())
                    enumerators.Remove(most_recent);
            }
        }
    }
}
