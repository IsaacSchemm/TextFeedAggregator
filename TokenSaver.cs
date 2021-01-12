using TextFeedAggregator.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TextFeedAggregator {
    public static class TokenSaver {
        public static async Task UpdateTokensAsync(ApplicationDbContext context, IdentityUser user, ExternalLoginInfo info) {
            if (info.LoginProvider == "DeviantArt") {
                var token = await context.UserDeviantArtTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token == null) {
                    token = new UserDeviantArtToken {
                        UserId = user.Id
                    };
                    context.UserDeviantArtTokens.Add(token);
                }
                token.AccessToken = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token")
                    .Select(t => t.Value)
                    .Single();
                token.RefreshToken = info.AuthenticationTokens
                    .Where(t => t.Name == "refresh_token")
                    .Select(t => t.Value)
                    .Single();
                await context.SaveChangesAsync();
            } else if (info.LoginProvider == "Twitter") {
                var token = await context.UserTwitterTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token == null) {
                    token = new UserTwitterToken {
                        UserId = user.Id
                    };
                    context.UserTwitterTokens.Add(token);
                }
                token.AccessToken = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token")
                    .Select(t => t.Value)
                    .Single();
                token.AccessTokenSecret = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token_secret")
                    .Select(t => t.Value)
                    .Single();
                await context.SaveChangesAsync();
            } else if (new[] { "mastodon.social", "mastodon.technology" }.Contains(info.LoginProvider)) {
                var token = await context.UserMastodonTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token == null) {
                    token = new UserMastodonToken {
                        UserId = user.Id,
                        Host = info.LoginProvider
                    };
                    context.UserMastodonTokens.Add(token);
                }
                token.AccessToken = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token")
                    .Select(t => t.Value)
                    .Single();
                await context.SaveChangesAsync();
            }
        }

        public static async Task RemoveTokensAsync(ApplicationDbContext context, IdentityUser user, string loginProvider) {
            if (loginProvider == "DeviantArt") {
                var token = await context.UserDeviantArtTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token != null) {
                    context.Remove(token);
                    await context.SaveChangesAsync();
                }
            } else if (loginProvider == "Twitter") {
                var token = await context.UserTwitterTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token != null) {
                    context.Remove(token);
                    await context.SaveChangesAsync();
                }
            } else if (new[] { "mastodon.social", "mastodon.technology" }.Contains(loginProvider)) {
                var tokens = await context.UserMastodonTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .Where(t => t.Host == loginProvider)
                    .ToListAsync();
                if (tokens.Any()) {
                    context.RemoveRange(tokens);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
