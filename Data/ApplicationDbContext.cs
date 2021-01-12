using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TextFeedAggregator.Data {
    public class ApplicationDbContext : IdentityDbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {
        }

        public DbSet<UserDeviantArtToken> UserDeviantArtTokens { get; set; }
        public DbSet<UserMastodonToken> UserMastodonTokens { get; set; }
        public DbSet<UserTwitterToken> UserTwitterTokens { get; set; }
    }
}
