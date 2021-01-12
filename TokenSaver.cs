using TextFeedAggregator.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TextFeedAggregator {
    public class TokenSaver {
        private readonly ApplicationDbContext _context;

        public TokenSaver(ApplicationDbContext context) {
            _context = context;
        }

        public async Task UpdateTokensAsync(IdentityUser user, ExternalLoginInfo info) {
            if (info.LoginProvider == "DeviantArt") {
                var token = await _context.UserDeviantArtTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token == null) {
                    token = new UserDeviantArtToken {
                        UserId = user.Id
                    };
                    _context.UserDeviantArtTokens.Add(token);
                }
                token.AccessToken = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token")
                    .Select(t => t.Value)
                    .Single();
                token.RefreshToken = info.AuthenticationTokens
                    .Where(t => t.Name == "refresh_token")
                    .Select(t => t.Value)
                    .Single();
                await _context.SaveChangesAsync();
            } else if (info.LoginProvider == "Twitter") {
                var token = await _context.UserTwitterTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token == null) {
                    token = new UserTwitterToken {
                        UserId = user.Id
                    };
                    _context.UserTwitterTokens.Add(token);
                }
                token.AccessToken = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token")
                    .Select(t => t.Value)
                    .Single();
                token.AccessTokenSecret = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token_secret")
                    .Select(t => t.Value)
                    .Single();
                await _context.SaveChangesAsync();
            } else if (new[] { "mastodon.social", "mastodon.technology" }.Contains(info.LoginProvider)) {
                var token = await _context.UserMastodonTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token == null) {
                    token = new UserMastodonToken {
                        UserId = user.Id,
                        Host = info.LoginProvider
                    };
                    _context.UserMastodonTokens.Add(token);
                }
                token.AccessToken = info.AuthenticationTokens
                    .Where(t => t.Name == "access_token")
                    .Select(t => t.Value)
                    .Single();
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveTokensAsync(IdentityUser user, string loginProvider) {
            if (loginProvider == "DeviantArt") {
                var token = await _context.UserDeviantArtTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token != null) {
                    _context.Remove(token);
                    await _context.SaveChangesAsync();
                }
            } else if (loginProvider == "Twitter") {
                var token = await _context.UserTwitterTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .SingleOrDefaultAsync();
                if (token != null) {
                    _context.Remove(token);
                    await _context.SaveChangesAsync();
                }
            } else if (new[] { "mastodon.social", "mastodon.technology" }.Contains(loginProvider)) {
                var tokens = await _context.UserMastodonTokens
                    .AsQueryable()
                    .Where(t => t.UserId == user.Id)
                    .Where(t => t.Host == loginProvider)
                    .ToListAsync();
                if (tokens.Any()) {
                    _context.RemoveRange(tokens);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
