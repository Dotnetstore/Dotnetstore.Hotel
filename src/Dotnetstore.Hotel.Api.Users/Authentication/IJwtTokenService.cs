using Dotnetstore.Hotel.Api.Users.Domain;

namespace Dotnetstore.Hotel.Api.Users.Authentication;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAtUtc) GenerateToken(ApplicationUser user, IReadOnlyCollection<string> roles);
}
