namespace Dotnetstore.Hotel.Ui.Web.Authentication;

/// <summary>
/// Names used with <see cref="Microsoft.AspNetCore.Authentication.AuthenticationProperties.StoreTokens"/> /
/// <see cref="Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.GetTokenAsync(Microsoft.AspNetCore.Http.HttpContext, string)"/>
/// to keep the access/refresh token pair alongside the auth cookie.
/// </summary>
public static class AuthTokenNames
{
    public const string AccessToken = "access_token";
    public const string RefreshToken = "refresh_token";
    public const string ExpiresAt = "expires_at";
}
