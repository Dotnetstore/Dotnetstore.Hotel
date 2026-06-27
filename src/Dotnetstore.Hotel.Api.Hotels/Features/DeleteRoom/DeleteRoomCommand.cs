using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Features.DeleteRoom;

public record DeleteRoomCommand(Guid Id) : ICommand<DeleteRoomResponse>;
