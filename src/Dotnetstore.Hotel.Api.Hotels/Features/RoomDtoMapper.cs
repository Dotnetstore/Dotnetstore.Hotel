using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;
using RoomEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Features;

/// <summary>
/// Shared "build a RoomDto for this room" mapping, used by every Room feature.
/// Synchronous, same style as EquipmentDtoMapper.
/// </summary>
internal static class RoomDtoMapper
{
    public static RoomDto ToDto(RoomEntity room) => new(
        room.Id,
        room.RoomNumber,
        room.Floor,
        room.Capacity,
        room.BedType,
        room.PricePerNight,
        room.Status,
        room.Equipment
            .Select(re => new RoomEquipmentDto(EquipmentDtoMapper.ToDto(re.Equipment), re.Tags.Select(TagDtoMapper.ToDto).ToList()))
            .ToList());
}
