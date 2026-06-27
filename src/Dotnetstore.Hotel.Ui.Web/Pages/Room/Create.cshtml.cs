using System.ComponentModel.DataAnnotations;
using Dotnetstore.Hotel.Shared.Sdk.Client;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages.Room;

[Authorize(Roles = "administrator,superuser")]
public class CreateModel(IHotelClient hotelClient) : PageModel
{
    // Mirrors Api.Hotels' RoomStatuses constants - duplicated rather than shared, since the UI project
    // doesn't reference Api.Hotels (same trade-off already accepted for the "administrator"/"superuser"
    // role literals duplicated between Api.Hotels and Api.Users).
    public IReadOnlyList<string> Statuses { get; } = ["Available", "Maintenance", "OutOfService"];

    [BindProperty, Required]
    public string RoomNumber { get; set; } = string.Empty;

    [BindProperty]
    public int Floor { get; set; }

    [BindProperty]
    public int Capacity { get; set; } = 1;

    [BindProperty, Required]
    public string BedType { get; set; } = string.Empty;

    [BindProperty]
    public decimal PricePerNight { get; set; }

    [BindProperty, Required]
    public string Status { get; set; } = "Available";

    [BindProperty]
    public List<EquipmentSelectionRow> EquipmentSelections { get; set; } = [];

    public IReadOnlyList<TagDto> AvailableTags { get; private set; } = [];

    public IReadOnlyList<string> Errors { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var equipment = await hotelClient.ListEquipmentAsync(accessToken, cancellationToken);
        AvailableTags = await hotelClient.ListTagsAsync(accessToken, cancellationToken);
        EquipmentSelections = equipment
            .Select(e => new EquipmentSelectionRow { EquipmentId = e.Id, EquipmentName = e.Name })
            .ToList();

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
            AvailableTags = await hotelClient.ListTagsAsync(accessToken, cancellationToken);
            return Page();
        }

        var equipmentItems = EquipmentSelections
            .Where(row => row.Included)
            .Select(row => new RoomEquipmentInput(row.EquipmentId, row.SelectedTagIds))
            .ToList();

        var request = new CreateRoomRequest(RoomNumber, Floor, Capacity, BedType, PricePerNight, Status, equipmentItems);
        var result = await hotelClient.CreateRoomAsync(request, accessToken, cancellationToken);
        if (result.Room is null)
        {
            Errors = result.Errors;
            AvailableTags = await hotelClient.ListTagsAsync(accessToken, cancellationToken);
            return Page();
        }

        TempData["StatusMessage"] = $"Room '{result.Room.RoomNumber}' created.";
        return RedirectToPage("/Room/Index");
    }

    public class EquipmentSelectionRow
    {
        public Guid EquipmentId { get; set; }

        public string EquipmentName { get; set; } = string.Empty;

        public bool Included { get; set; }

        public List<Guid> SelectedTagIds { get; set; } = [];
    }
}
