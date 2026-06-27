namespace Dotnetstore.Hotel.Ui.Web.Options;

/// <summary>
/// The Hotels API has no "list hotels" endpoint yet (single-hotel system) so the UI is configured with the
/// id of the hotel record to display/edit, defaulting to the seeded hotel's well-known id.
/// </summary>
public class HotelUiOptions
{
    public Guid Id { get; set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
}
