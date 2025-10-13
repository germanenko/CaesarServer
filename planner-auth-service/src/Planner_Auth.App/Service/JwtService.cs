using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Planner_Auth.Core.Entities.Response;
using Planner_Auth.Core.IService;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Planner_Auth.App.Service
{
    public class JwtService : IJwtService
    {
        private readonly SigningCredentials _signingCredentials;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly IMemoryCache _cache;

        public JwtService(string key, string issuer, string audience, IMemoryCache cache)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            _issuer = issuer;
            _audience = audience;
            _cache = cache;
        }

        private string GenerateAccessToken(Dictionary<string, string> claims, TimeSpan timeSpan)
        {
            var tokenClaims = claims.Select(claim => new Claim(claim.Key, claim.Value));

            var token = new JwtSecurityToken(
                claims: tokenClaims,
                expires: DateTime.UtcNow.Add(timeSpan),
                signingCredentials: _signingCredentials,
                issuer: _issuer,
                audience: _audience
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken() => Guid.NewGuid().ToString();

        public OutputAccountCredentialsBody GenerateDefaultTokenPair(TokenPayload tokenPayload)
        {
            var claims = new Dictionary<string, string>{
                { "AccountId", tokenPayload.AccountId.ToString() },
                { "SessionId", tokenPayload.SessionId.ToString() },
                { ClaimTypes.Role, tokenPayload.Role},
            };
            var timeSpan = new TimeSpan(2, 0, 0, 0);
            return GenerateTokenPair(claims, timeSpan);
        }

        private OutputAccountCredentialsBody GenerateTokenPair(Dictionary<string, string> claims, TimeSpan timeSpan) =>
            new(
                    GenerateAccessToken(claims, timeSpan),
                    GenerateRefreshToken()
                );

        private List<Claim> GetClaims(string token) =>
            new JwtSecurityTokenHandler()
                .ReadJwtToken(token.Replace("Bearer ", ""))
                .Claims
                .ToList();

        public TokenPayload GetTokenPayload(string token)
        {
            var claims = GetClaims(token);
            return new TokenPayload
            {
                Role = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value,
                AccountId = Guid.Parse(claims.FirstOrDefault(claim => claim.Type == "AccountId")?.Value),
                SessionId = Guid.Parse(claims.FirstOrDefault(claim => claim.Type == "SessionId")?.Value),
            };
        }

        public string GeneratePasswordResetToken(string userId)
        {
            var expiration = TimeSpan.FromHours(1);

            var tokenId = Guid.NewGuid().ToString();

            var claims = new Dictionary<string, string>
            {
                { "userId", userId },
                { "purpose", "password_reset" }, 
                { "tokenId", tokenId } 
            };

            _cache.Set($"reset_token_{tokenId}", true, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });

            var timeSpan = expiration;

            return GenerateAccessToken(claims, timeSpan);
        }

        // Валидация токена сброса пароля
        public bool ValidatePasswordResetToken(string token, string expectedEmail = null)
        {
            try
            {
                var claims = GetClaims(token);

                var purpose = claims.FirstOrDefault(c => c.Type == "purpose")?.Value;
                if (purpose != "password_reset")
                    return false;

                var expClaim = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
                if (expClaim != null && long.TryParse(expClaim, out long exp))
                {
                    var expiryDate = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                    if (expiryDate < DateTime.UtcNow)
                        return false;
                }
   
                var tokenId = claims.FirstOrDefault(c => c.Type == "tokenId")?.Value;
                if (string.IsNullOrEmpty(tokenId))
                    return false;

                _cache.TryGetValue($"reset_token_{tokenId}", out bool inCache);

                if (!inCache)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public PasswordResetTokenPayload GetPasswordResetTokenPayload(string token)
        {
            try
            {
                var claims = GetClaims(token);

                if (!ValidatePasswordResetToken(token))
                {
                    return null;
                }

                return new PasswordResetTokenPayload
                {
                    UserId = Guid.Parse(claims.FirstOrDefault(c => c.Type == "userId")?.Value),
                    TokenId = claims.FirstOrDefault(c => c.Type == "tokenId")?.Value,
                    ExpiresAt = GetTokenExpiration(token)
                };
            }
            catch
            {
                return null;
            }
        }

        public DateTime GetTokenExpiration(string token)
        {
            var claims = GetClaims(token);
            var expClaim = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            return expClaim != null && long.TryParse(expClaim, out long exp)
                ? DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime
                : DateTime.MinValue;
        }

        public void InvalidatePasswordResetToken(string token)
        {
            var claims = GetClaims(token);
            var tokenId = claims.FirstOrDefault(c => c.Type == "tokenId")?.Value;

            if (!string.IsNullOrEmpty(tokenId))
            {
                _cache.Remove($"reset_token_{tokenId}");
            }
        }
    }
}