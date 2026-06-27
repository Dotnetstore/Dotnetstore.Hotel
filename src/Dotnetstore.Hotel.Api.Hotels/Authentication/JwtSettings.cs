namespace Dotnetstore.Hotel.Api.Hotels.Authentication;

/// <summary>
/// Api.Hotels only validates JWTs issued by Api.Users (never issues its own), so it only needs the
/// signing key/issuer/audience to check against - no expiry/refresh settings like Api.Users has.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = "dotnetstore-hotel";

    public string Audience { get; set; } = "dotnetstore-hotel";
}
