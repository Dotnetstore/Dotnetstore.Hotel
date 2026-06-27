using Dotnetstore.Hotel.Shared.Sdk.Client;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages.Bookings;

[Authorize(Roles = "administrator,superuser,desk")]
public class CreateModel(IHotelClient hotelClient) : PageModel
{
    [BindProperty]
    public DateOnly? CheckInDate { get; set; }

    [BindProperty]
    public DateOnly? CheckOutDate { get; set; }

    [BindProperty]
    public List<Guid> SelectedEquipmentIds { get; set; } = [];

    [BindProperty]
    public List<Guid> SelectedTagIds { get; set; } = [];

    [BindProperty]
    public List<Guid> SelectedRoomIds { get; set; } = [];

    [BindProperty]
    public string? CustomerSearch { get; set; }

    [BindProperty]
    public Guid? SelectedCustomerId { get; set; }

    public IReadOnlyList<EquipmentDto> AvailableEquipment { get; private set; } = [];

    public IReadOnlyList<TagDto> AvailableTags { get; private set; } = [];

    public IReadOnlyList<RoomDto> MatchingRooms { get; private set; } = [];

    public IReadOnlyList<CustomerDto> MatchingCustomers { get; private set; } = [];

    public IReadOnlyList<string> Errors { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        await LoadFiltersAndSearchResultsAsync(accessToken, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostSearchAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        await LoadFiltersAndSearchResultsAsync(accessToken, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        if (SelectedCustomerId is null)
        {
            Errors = ["Select a customer first."];
            await LoadFiltersAndSearchResultsAsync(accessToken, cancellationToken);
            return Page();
        }

        if (SelectedRoomIds.Count == 0)
        {
            Errors = ["Select at least one room first."];
            await LoadFiltersAndSearchResultsAsync(accessToken, cancellationToken);
            return Page();
        }

        if (CheckInDate is null || CheckOutDate is null)
        {
            Errors = ["Check-in and check-out dates are required."];
            await LoadFiltersAndSearchResultsAsync(accessToken, cancellationToken);
            return Page();
        }

        var request = new CreateBookingRequest(SelectedCustomerId.Value, CheckInDate.Value, CheckOutDate.Value, SelectedRoomIds);
        var result = await hotelClient.CreateBookingAsync(request, accessToken, cancellationToken);
        if (result.Booking is null)
        {
            Errors = result.Errors;
            await LoadFiltersAndSearchResultsAsync(accessToken, cancellationToken);
            return Page();
        }

        TempData["StatusMessage"] = $"Booking created for {result.Booking.Customer.FullName}.";
        return RedirectToPage("/Bookings/Index");
    }

    private async Task LoadFiltersAndSearchResultsAsync(string accessToken, CancellationToken cancellationToken)
    {
        AvailableEquipment = await hotelClient.ListEquipmentAsync(accessToken, cancellationToken);
        AvailableTags = await hotelClient.ListTagsAsync(accessToken, cancellationToken);
        MatchingRooms = await hotelClient.SearchRoomsAsync(CheckInDate, CheckOutDate, SelectedEquipmentIds, SelectedTagIds, accessToken, cancellationToken);

        MatchingCustomers = string.IsNullOrWhiteSpace(CustomerSearch)
            ? []
            : await hotelClient.ListCustomersAsync(CustomerSearch, accessToken, cancellationToken);
    }
}
