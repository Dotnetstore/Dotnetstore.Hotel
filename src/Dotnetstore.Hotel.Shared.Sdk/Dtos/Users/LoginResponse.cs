namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

public record LoginResponse(string Token, DateTimeOffset ExpiresAtUtc, string RefreshToken, DateTimeOffset RefreshTokenExpiresAtUtc);
