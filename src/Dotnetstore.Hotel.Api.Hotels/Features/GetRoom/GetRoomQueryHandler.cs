using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Features.GetRoom;

public class GetRoomQueryHandler(IRoomRepository roomRepository) : IQueryHandler<GetRoomQuery, RoomDto?>
{
    public async Task<RoomDto?> HandleAsync(GetRoomQuery query, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetByIdAsync(query.Id, cancellationToken);
        return room is null ? null : RoomDtoMapper.ToDto(room);
    }
}
