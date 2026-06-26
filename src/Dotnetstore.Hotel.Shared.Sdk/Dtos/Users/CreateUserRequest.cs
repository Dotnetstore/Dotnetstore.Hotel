namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

public record CreateUserRequest(string Email, string UserName, string Password, string Role);
