using System.ComponentModel.DataAnnotations;
using Dotnetstore.Hotel.Shared.Sdk.Client.Users;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages;

public class LoginModel(IUserClient userClient) : PageModel
{
    [BindProperty]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string Password { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? Error { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var login = await userClient.LoginAsync(new LoginRequest(Email, Password), HttpContext.RequestAborted);
        if (login is null)
        {
            Error = "Incorrect email or password.";
            return Page();
        }

        var principal = JwtPrincipalFactory.CreatePrincipal(login.Token, CookieAuthenticationDefaults.AuthenticationScheme);
        var properties = new AuthenticationProperties { IsPersistent = true };
        properties.StoreTokens(
        [
            new AuthenticationToken { Name = AuthTokenNames.AccessToken, Value = login.Token },
            new AuthenticationToken { Name = AuthTokenNames.RefreshToken, Value = login.RefreshToken },
            new AuthenticationToken { Name = AuthTokenNames.ExpiresAt, Value = login.ExpiresAtUtc.ToString("o") },
        ]);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

        return LocalRedirect(Url.IsLocalUrl(ReturnUrl) && ReturnUrl is not null ? ReturnUrl : "/Hotel");
    }
}
