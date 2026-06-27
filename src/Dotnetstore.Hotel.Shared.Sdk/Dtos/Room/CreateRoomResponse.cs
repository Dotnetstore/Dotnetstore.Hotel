namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

public record CreateRoomResponse(RoomDto? Room, IReadOnlyList<string> Errors);
