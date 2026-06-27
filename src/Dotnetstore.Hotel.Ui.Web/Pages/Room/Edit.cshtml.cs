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
public class EditModel(IHotelClient hotelClient) : PageModel
{
    public IReadOnlyList<string> Statuses { get; } = ["Available", "Maintenance", "OutOfService"];

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

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
    public List<CreateModel.EquipmentSelectionRow> EquipmentSelections { get; set; } = [];

    public IReadOnlyList<TagDto> AvailableTags { get; private set; } = [];

    public IReadOnlyList<string> Errors { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var room = await hotelClient.GetRoomAsync(Id, accessToken, cancellationToken);
        if (room is null)
        {
            return NotFound();
        }

        RoomNumber = room.RoomNumber;
        Floor = room.Floor;
        Capacity = room.Capacity;
        BedType = room.BedType;
        PricePerNight = room.PricePerNight;
        Status = room.Status;

        var equipment = await hotelClient.ListEquipmentAsync(accessToken, cancellationToken);
        AvailableTags = await hotelClient.ListTagsAsync(accessToken, cancellationToken);

        var roomEquipmentById = room.Equipment.ToDictionary(re => re.Equipment.Id);
        EquipmentSelections = equipment
            .Select(e => new CreateModel.EquipmentSelectionRow
            {
                EquipmentId = e.Id,
                EquipmentName = e.Name,
                Included = roomEquipmentById.ContainsKey(e.Id),
                SelectedTagIds = roomEquipmentById.TryGetValue(e.Id, out var roomEquipment)
                    ? roomEquipment.Tags.Select(t => t.Id).ToList()
                    : [],
            })
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

        var request = new UpdateRoomRequest(RoomNumber, Floor, Capacity, BedType, PricePerNight, Status, equipmentItems);
        var result = await hotelClient.UpdateRoomAsync(Id, request, accessToken, cancellationToken);
        if (result.Room is null)
        {
            Errors = result.Errors;
            AvailableTags = await hotelClient.ListTagsAsync(accessToken, cancellationToken);
            return Page();
        }

        TempData["StatusMessage"] = $"Room '{result.Room.RoomNumber}' updated.";
        return RedirectToPage("/Room/Index");
    }
}
