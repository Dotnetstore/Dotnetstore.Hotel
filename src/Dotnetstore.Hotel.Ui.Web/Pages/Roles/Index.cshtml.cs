using Dotnetstore.Hotel.Shared.Sdk.Client.Users;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages.Roles;

[Authorize(Roles = "administrator,superuser")]
public class IndexModel(IUserClient userClient) : PageModel
{
    public IReadOnlyList<RoleDto> Roles { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        Roles = await userClient.ListRolesAsync(accessToken, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var result = await userClient.DeleteRoleAsync(id, accessToken, cancellationToken);
        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] = result.Succeeded
            ? "Role deleted."
            : string.Join(" ", result.Errors);

        return RedirectToPage();
    }
}
