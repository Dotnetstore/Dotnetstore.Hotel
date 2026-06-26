namespace Dotnetstore.Hotel.Api.Users.Authentication;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = "dotnetstore-hotel";

    public string Audience { get; set; } = "dotnetstore-hotel";

    public TimeSpan Expiry { get; set; } = TimeSpan.FromHours(1);

    public TimeSpan RefreshTokenExpiry { get; set; } = TimeSpan.FromDays(14);
}
