using Dotnetstore.Hotel.Shared.Sdk.Client;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages.Room;

[Authorize]
public class IndexModel(IHotelClient hotelClient) : PageModel
{
    public IReadOnlyList<RoomDto> Rooms { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        Rooms = await hotelClient.ListRoomsAsync(accessToken, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var result = await hotelClient.DeleteRoomAsync(id, accessToken, cancellationToken);
        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] = result.Succeeded
            ? "Room deleted."
            : string.Join(" ", result.Errors);

        return RedirectToPage();
    }
}
