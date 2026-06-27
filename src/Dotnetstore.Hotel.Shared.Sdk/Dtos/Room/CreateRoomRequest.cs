namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

public record CreateRoomRequest(
    string RoomNumber,
    int Floor,
    int Capacity,
    string BedType,
    decimal PricePerNight,
    string Status,
    IReadOnlyCollection<RoomEquipmentInput> Equipment);
