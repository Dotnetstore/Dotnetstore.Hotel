using System.ComponentModel.DataAnnotations;
using Dotnetstore.Hotel.Shared.Sdk.Client.Users;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages.Roles;

[Authorize(Roles = "administrator,superuser")]
public class EditModel(IUserClient userClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty, Required]
    public string Name { get; set; } = string.Empty;

    public bool IsProtected { get; private set; }

    public IReadOnlyList<string> Errors { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var role = await userClient.GetRoleAsync(Id, accessToken, cancellationToken);
        if (role is null)
        {
            return NotFound();
        }

        Name = role.Name;
        IsProtected = role.IsProtected;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var result = await userClient.UpdateRoleAsync(Id, new UpdateRoleRequest(Name), accessToken, cancellationToken);
        if (result.Role is null)
        {
            Errors = result.Errors;
            return Page();
        }

        TempData["StatusMessage"] = $"Role '{result.Role.Name}' updated.";
        return RedirectToPage("/Roles/Index");
    }
}
