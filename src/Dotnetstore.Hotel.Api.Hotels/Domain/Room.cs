namespace Dotnetstore.Hotel.Api.Hotels.Domain;

public class Room
{
    private readonly List<RoomEquipment> _equipment = [];

    private Room()
    {
        RoomNumber = string.Empty;
        BedType = string.Empty;
        Status = RoomStatuses.Available;
    }

    private Room(Guid id, string roomNumber, int floor, int capacity, string bedType, decimal pricePerNight, string status)
    {
        Id = id;
        RoomNumber = roomNumber;
        Floor = floor;
        Capacity = capacity;
        BedType = bedType;
        PricePerNight = pricePerNight;
        Status = status;
    }

    public Guid Id { get; private set; }

    public string RoomNumber { get; private set; }

    public int Floor { get; private set; }

    public int Capacity { get; private set; }

    public string BedType { get; private set; }

    public decimal PricePerNight { get; private set; }

    public string Status { get; private set; }

    public IReadOnlyCollection<RoomEquipment> Equipment => _equipment;

    public static Room Create(Guid id, string roomNumber, int floor, int capacity, string bedType, decimal pricePerNight, string status)
        => new(id, roomNumber, floor, capacity, bedType, pricePerNight, status);

    public void UpdateDetails(string roomNumber, int floor, int capacity, string bedType, decimal pricePerNight, string status)
    {
        RoomNumber = roomNumber;
        Floor = floor;
        Capacity = capacity;
        BedType = bedType;
        PricePerNight = pricePerNight;
        Status = status;
    }

    public void SetEquipment(IEnumerable<(Equipment Equipment, IReadOnlyCollection<Tag> Tags)> items)
    {
        _equipment.Clear();
        _equipment.AddRange(items.Select(item => RoomEquipment.Create(Id, item.Equipment, item.Tags)));
    }
}
