using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Features.SearchRooms;

public record SearchRoomsQuery(
    DateOnly? CheckInDate,
    DateOnly? CheckOutDate,
    IReadOnlyCollection<Guid> EquipmentIds,
    IReadOnlyCollection<Guid> TagIds) : IQuery<IReadOnlyList<RoomDto>>;
