using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Features.ListRooms;

public class ListRoomsQueryHandler(IRoomRepository roomRepository) : IQueryHandler<ListRoomsQuery, IReadOnlyList<RoomDto>>
{
    public async Task<IReadOnlyList<RoomDto>> HandleAsync(ListRoomsQuery query, CancellationToken cancellationToken)
    {
        var rooms = await roomRepository.GetAllAsync(cancellationToken);
        return rooms.Select(RoomDtoMapper.ToDto).ToList();
    }
}
