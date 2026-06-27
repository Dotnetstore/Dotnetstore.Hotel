using Dotnetstore.Hotel.Shared.Sdk.Client.Users;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages.Users;

[Authorize(Roles = "administrator,superuser")]
public class IndexModel(IUserClient userClient) : PageModel
{
    public IReadOnlyList<UserDto> Users { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        Users = await userClient.ListUsersAsync(accessToken, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var succeeded = await userClient.DeactivateUserAsync(id, accessToken, cancellationToken);
        TempData[succeeded ? "StatusMessage" : "ErrorMessage"] = succeeded
            ? "User deactivated."
            : "Could not deactivate that user (not found, or it's your own account).";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        await userClient.ActivateUserAsync(id, accessToken, cancellationToken);
        TempData["StatusMessage"] = "User reactivated.";
        return RedirectToPage();
    }
}
