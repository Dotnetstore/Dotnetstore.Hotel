using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Features.ListRooms;

public record ListRoomsQuery : IQuery<IReadOnlyList<RoomDto>>;
