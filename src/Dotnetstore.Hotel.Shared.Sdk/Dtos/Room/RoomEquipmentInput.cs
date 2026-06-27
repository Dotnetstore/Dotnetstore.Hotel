namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

public record RoomEquipmentInput(Guid EquipmentId, IReadOnlyCollection<Guid> TagIds);
