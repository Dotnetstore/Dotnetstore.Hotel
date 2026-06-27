using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Features.GetRoom;

public record GetRoomQuery(Guid Id) : IQuery<RoomDto?>;
