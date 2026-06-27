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
public class EditModel(IUserClient userClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty, Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty, Required]
    public string UserName { get; set; } = string.Empty;

    [BindProperty, Required]
    public string Role { get; set; } = "desk";

    [BindProperty]
    public string? NewPassword { get; set; }

    public IReadOnlyList<string> AvailableRoles { get; private set; } = [];

    public IReadOnlyList<string> Errors { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var user = await userClient.GetUserAsync(Id, accessToken, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        Email = user.Email;
        UserName = user.UserName;
        Role = user.Roles.FirstOrDefault() ?? "desk";
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

        var request = new UpdateUserRequest(Email, UserName, Role, string.IsNullOrWhiteSpace(NewPassword) ? null : NewPassword);
        var result = await userClient.UpdateUserAsync(Id, request, accessToken, cancellationToken);
        if (result.User is null)
        {
            Errors = result.Errors;
            await LoadAvailableRolesAsync(accessToken, cancellationToken);
            return Page();
        }

        TempData["StatusMessage"] = $"User '{result.User.UserName}' updated.";
        return RedirectToPage("/Users/Index");
    }

    private async Task LoadAvailableRolesAsync(string accessToken, CancellationToken cancellationToken)
    {
        AvailableRoles = (await userClient.ListRolesAsync(accessToken, cancellationToken)).Select(r => r.Name).ToList();
    }
}
