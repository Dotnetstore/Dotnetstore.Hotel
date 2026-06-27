using System.ComponentModel.DataAnnotations;
using Dotnetstore.Hotel.Shared.Sdk.Client;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages.Equipment;

[Authorize(Roles = "administrator,superuser")]
public class CreateModel(IHotelClient hotelClient) : PageModel
{
    [BindProperty, Required]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string? Description { get; set; }

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

        var result = await hotelClient.CreateEquipmentAsync(new CreateEquipmentRequest(Name, Description), accessToken, cancellationToken);
        if (result.Equipment is null)
        {
            Errors = result.Errors;
            return Page();
        }

        TempData["StatusMessage"] = $"Equipment '{result.Equipment.Name}' created.";
        return RedirectToPage("/Equipment/Index");
    }
}
