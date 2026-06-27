using System.ComponentModel.DataAnnotations;
using Dotnetstore.Hotel.Shared.Sdk.Client;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages.Tags;

[Authorize(Roles = "administrator,superuser")]
public class CreateModel(IHotelClient hotelClient) : PageModel
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

        var result = await hotelClient.CreateTagAsync(new CreateTagRequest(Name), accessToken, cancellationToken);
        if (result.Tag is null)
        {
            Errors = result.Errors;
            return Page();
        }

        TempData["StatusMessage"] = $"Tag '{result.Tag.Name}' created.";
        return RedirectToPage("/Tags/Index");
    }
}
