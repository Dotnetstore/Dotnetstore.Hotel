namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

public record CreateUserResponse(UserDto? User, IReadOnlyList<string> Errors);
