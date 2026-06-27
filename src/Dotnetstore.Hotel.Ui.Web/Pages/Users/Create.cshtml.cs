using System.ComponentModel.DataAnnotations;
using Dotnetstore.Hotel.Shared.Sdk.Client.Users;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages.Users;

[Authorize(Roles = "administrator,superuser")]
public class CreateModel(IUserClient userClient) : PageModel
{
    [BindProperty, Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty, Required]
    public string UserName { get; set; } = string.Empty;

    [BindProperty, Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [BindProperty, Required]
    public string Role { get; set; } = "desk";

    public IReadOnlyList<string> AvailableRoles { get; private set; } = [];

    public IReadOnlyList<string> Errors { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        await LoadAvailableRolesAsync(accessToken, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            await LoadAvailableRolesAsync(accessToken, cancellationToken);
            return Page();
        }

        var result = await userClient.CreateUserAsync(new CreateUserRequest(Email, UserName, Password, Role), accessToken, cancellationToken);
        if (result.User is null)
        {
            Errors = result.Errors;
            await LoadAvailableRolesAsync(accessToken, cancellationToken);
            return Page();
        }

        TempData["StatusMessage"] = $"User '{result.User.UserName}' created with role '{Role}'.";
        return RedirectToPage("/Users/Index");
    }

    private async Task LoadAvailableRolesAsync(string accessToken, CancellationToken cancellationToken)
    {
        AvailableRoles = (await userClient.ListRolesAsync(accessToken, cancellationToken)).Select(r => r.Name).ToList();
    }
}
