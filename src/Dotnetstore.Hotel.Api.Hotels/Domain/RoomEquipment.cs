namespace Dotnetstore.Hotel.Api.Hotels.Domain;

/// <summary>
/// "This Equipment, in this Room" - the pairing itself, not just the link, since it carries its own
/// data (Tags) that varies per room even for the same Equipment catalog item (e.g. a hard bed in one
/// room, a soft bed of the same catalog type in another).
/// </summary>
public class RoomEquipment
{
    private readonly List<Tag> _tags = [];

    private RoomEquipment()
    {
    }

    private RoomEquipment(Guid roomId, Equipment equipment, IEnumerable<Tag> tags)
    {
        RoomId = roomId;
        EquipmentId = equipment.Id;
        Equipment = equipment;
        _tags.AddRange(tags);
    }

    public Guid RoomId { get; private set; }

    public Guid EquipmentId { get; private set; }

    public Equipment Equipment { get; private set; } = null!;

    public IReadOnlyCollection<Tag> Tags => _tags;

    public static RoomEquipment Create(Guid roomId, Equipment equipment, IEnumerable<Tag> tags) => new(roomId, equipment, tags);

    public void SetTags(IEnumerable<Tag> tags)
    {
        _tags.Clear();
        _tags.AddRange(tags);
    }
}
