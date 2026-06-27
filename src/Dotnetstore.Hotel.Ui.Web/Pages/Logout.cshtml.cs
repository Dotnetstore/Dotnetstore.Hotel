using Dotnetstore.Hotel.Shared.Sdk.Client.Users;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages;

public class LogoutModel(IUserClient userClient) : PageModel
{
    public async Task<IActionResult> OnPostAsync()
    {
        var refreshToken = await HttpContext.GetTokenAsync(AuthTokenNames.RefreshToken);
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await userClient.LogoutAsync(refreshToken, HttpContext.RequestAborted);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Login");
    }
}
