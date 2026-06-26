using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dotnetstore.Hotel.Api.Users.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Dotnetstore.Hotel.Api.Users.Authentication;

public class JwtTokenService(IOptions<JwtSettings> jwtSettings) : IJwtTokenService
{
    public (string Token, DateTimeOffset ExpiresAtUtc) GenerateToken(ApplicationUser user, IReadOnlyCollection<string> roles)
    {
        var settings = jwtSettings.Value;
        var expiresAtUtc = DateTimeOffset.UtcNow.Add(settings.Expiry);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
