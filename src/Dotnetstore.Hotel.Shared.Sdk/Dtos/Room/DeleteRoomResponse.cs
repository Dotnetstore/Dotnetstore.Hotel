namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

public record DeleteRoomResponse(bool Succeeded, IReadOnlyList<string> Errors);
