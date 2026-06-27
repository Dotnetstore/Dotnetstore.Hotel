using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;

namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

public record RoomEquipmentDto(EquipmentDto Equipment, IReadOnlyList<TagDto> Tags);
