namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

public record UpdateRoomResponse(RoomDto? Room, IReadOnlyList<string> Errors);
