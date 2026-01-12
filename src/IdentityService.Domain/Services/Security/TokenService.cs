using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using IdentityService.Domain.Configuration;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces.Security;

namespace IdentityService.Domain.Services.Security;

public class TokenService(IOptions<TokenConfiguration> options, IMemoryCache cache) : ITokenService
{
    private readonly TokenConfiguration _configuration = options.Value;
    private readonly IMemoryCache _cache = cache;

    public string GenerateToken(User user, bool force = false)
    {
        if (_cache.TryGetValue(user.Id, out string? token) && token is not null && force == false)
            return token;
        else
            _cache.Remove(user.Id);

        token = CreateToken(user);

        var cacheOptions = new MemoryCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(_configuration.ExpirationTimeHour),
            SlidingExpiration = TimeSpan.FromMinutes(_configuration.IncreaseExpirationTimeMinutes)
        };

        _cache.Set(user.Id, token, cacheOptions);

        return token;
    }

    private string CreateToken(User user)
    {
        var jwtKey = _configuration.Key;
        if (string.IsNullOrEmpty(jwtKey))
            throw new Exception("JWT Key is not configured.");

        var key = Convert.FromBase64String(jwtKey);
        var tokenHandler = new JwtSecurityTokenHandler();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("sub", user.Id.ToString()),
            new("userId", user.Id.ToString()),
            new(ClaimTypes.Name, user.Name ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        if (user.AccessLevel.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.Role, ((int)user.AccessLevel.Value).ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _configuration.Issuer,
            Expires = DateTime.UtcNow.AddHours(_configuration.ExpirationTimeHour),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
