using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Features.SearchRooms;

public class SearchRoomsQueryHandler(IRoomRepository roomRepository, IBookingRepository bookingRepository)
    : IQueryHandler<SearchRoomsQuery, IReadOnlyList<RoomDto>>
{
    public async Task<IReadOnlyList<RoomDto>> HandleAsync(SearchRoomsQuery query, CancellationToken cancellationToken)
    {
        var rooms = await roomRepository.GetAllAsync(cancellationToken);

        var matching = rooms.Where(room =>
        {
            if (query.EquipmentIds.Count > 0)
            {
                var roomEquipmentIds = room.Equipment.Select(re => re.EquipmentId).ToHashSet();
                if (!query.EquipmentIds.All(roomEquipmentIds.Contains))
                {
                    return false;
                }
            }

            if (query.TagIds.Count > 0)
            {
                var roomTagIds = room.Equipment.SelectMany(re => re.Tags.Select(t => t.Id)).ToHashSet();
                if (!query.TagIds.All(roomTagIds.Contains))
                {
                    return false;
                }
            }

            return true;
        }).ToList();

        if (query.CheckInDate is { } checkIn && query.CheckOutDate is { } checkOut)
        {
            var available = new List<Domain.Room>();
            foreach (var room in matching)
            {
                if (!await bookingRepository.HasOverlappingBookingAsync(room.Id, checkIn, checkOut, excludingBookingId: null, cancellationToken))
                {
                    available.Add(room);
                }
            }

            matching = available;
        }

        return matching.Select(RoomDtoMapper.ToDto).ToList();
    }
}
