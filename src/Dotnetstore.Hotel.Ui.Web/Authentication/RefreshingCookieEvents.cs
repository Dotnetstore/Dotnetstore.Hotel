using Dotnetstore.Hotel.Shared.Sdk.Client.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnetstore.Hotel.Ui.Web.Authentication;

/// <summary>
/// On every request, renews the access/refresh token pair shortly before the access token expires, so a logged-in
/// user is never bounced to the login page just because their JWT's short lifetime ran out mid-session.
/// </summary>
public class RefreshingCookieEvents : CookieAuthenticationEvents
{
    private static readonly TimeSpan RefreshThreshold = TimeSpan.FromMinutes(1);

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var expiresAtRaw = context.Properties.GetTokenValue(AuthTokenNames.ExpiresAt);
        if (DateTimeOffset.TryParse(expiresAtRaw, out var expiresAt) && expiresAt > DateTimeOffset.UtcNow.Add(RefreshThreshold))
        {
            return;
        }

        var refreshToken = context.Properties.GetTokenValue(AuthTokenNames.RefreshToken);
        if (string.IsNullOrEmpty(refreshToken))
        {
            context.RejectPrincipal();
            return;
        }

        var userClient = context.HttpContext.RequestServices.GetRequiredService<IUserClient>();
        var refreshed = await userClient.RefreshTokenAsync(refreshToken, context.HttpContext.RequestAborted);
        if (refreshed is null)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        context.ReplacePrincipal(JwtPrincipalFactory.CreatePrincipal(refreshed.Token, context.Scheme.Name));
        context.ShouldRenew = true;
        context.Properties.UpdateTokenValue(AuthTokenNames.AccessToken, refreshed.Token);
        context.Properties.UpdateTokenValue(AuthTokenNames.RefreshToken, refreshed.RefreshToken);
        context.Properties.UpdateTokenValue(AuthTokenNames.ExpiresAt, refreshed.ExpiresAtUtc.ToString("o"));
    }
}
