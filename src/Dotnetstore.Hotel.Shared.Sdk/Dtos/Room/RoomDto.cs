namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

public record RoomDto(
    Guid Id,
    string RoomNumber,
    int Floor,
    int Capacity,
    string BedType,
    decimal PricePerNight,
    string Status,
    IReadOnlyList<RoomEquipmentDto> Equipment);
