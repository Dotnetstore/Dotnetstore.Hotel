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
public class EditModel(IHotelClient hotelClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty, Required]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string? Description { get; set; }

    public IReadOnlyList<string> Errors { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var equipment = await hotelClient.GetEquipmentAsync(Id, accessToken, cancellationToken);
        if (equipment is null)
        {
            return NotFound();
        }

        Name = equipment.Name;
        Description = equipment.Description;
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

        var result = await hotelClient.UpdateEquipmentAsync(Id, new UpdateEquipmentRequest(Name, Description), accessToken, cancellationToken);
        if (result.Equipment is null)
        {
            Errors = result.Errors;
            return Page();
        }

        TempData["StatusMessage"] = $"Equipment '{result.Equipment.Name}' updated.";
        return RedirectToPage("/Equipment/Index");
    }
}
