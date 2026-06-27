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
public class CreateModel(IUserClient userClient) : PageModel
{
    [BindProperty, Required]
    public string Name { get; set; } = string.Empty;

    public IReadOnlyList<string> Errors { get; private set; } = [];

    public void OnGet()
    {
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

        var result = await userClient.CreateRoleAsync(new CreateRoleRequest(Name), accessToken, cancellationToken);
        if (result.Role is null)
        {
            Errors = result.Errors;
            return Page();
        }

        TempData["StatusMessage"] = $"Role '{result.Role.Name}' created.";
        return RedirectToPage("/Roles/Index");
    }
}
