using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Dotnetstore.Hotel.Ui.Web.Authentication;

/// <summary>
/// Builds the UI's <see cref="ClaimsPrincipal"/> directly from the access token's claims (sub/email/name/role),
/// without re-validating the signature - the token was just received from our own Users API over HTTPS, so the
/// claims are already trusted at this point. The signature still protects the token on every subsequent API call.
/// </summary>
public static class JwtPrincipalFactory
{
    public static ClaimsPrincipal CreatePrincipal(string accessToken, string authenticationScheme)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        var identity = new ClaimsIdentity(jwt.Claims, authenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
        return new ClaimsPrincipal(identity);
    }
}
