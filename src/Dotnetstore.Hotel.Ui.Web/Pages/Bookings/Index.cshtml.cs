using Dotnetstore.Hotel.Shared.Sdk.Client;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages.Bookings;

[Authorize(Roles = "administrator,superuser,desk")]
public class IndexModel(IHotelClient hotelClient) : PageModel
{
    public IReadOnlyList<BookingDto> Bookings { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        Bookings = await hotelClient.ListBookingsAsync(null, accessToken, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var result = await hotelClient.CancelBookingAsync(id, accessToken, cancellationToken);
        SetResultMessage(result, "Booking cancelled.");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCheckInAsync(Guid id, CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var result = await hotelClient.CheckInBookingAsync(id, accessToken, cancellationToken);
        SetResultMessage(result, "Checked in.");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCheckOutAsync(Guid id, CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var result = await hotelClient.CheckOutBookingAsync(id, accessToken, cancellationToken);
        SetResultMessage(result, "Checked out.");
        return RedirectToPage();
    }

    private void SetResultMessage(BookingActionResponse result, string successMessage)
    {
        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] = result.Succeeded
            ? successMessage
            : string.Join(" ", result.Errors);
    }
}
