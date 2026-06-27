using Dotnetstore.Hotel.Shared.Sdk.Client;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages.Equipment;

[Authorize(Roles = "administrator,superuser")]
public class IndexModel(IHotelClient hotelClient) : PageModel
{
    public IReadOnlyList<EquipmentDto> Equipment { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        Equipment = await hotelClient.ListEquipmentAsync(accessToken, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var result = await hotelClient.DeleteEquipmentAsync(id, accessToken, cancellationToken);
        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] = result.Succeeded
            ? "Equipment deleted."
            : string.Join(" ", result.Errors);

        return RedirectToPage();
    }
}
