namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

public record UpdateUserResponse(UserDto? User, IReadOnlyList<string> Errors);
