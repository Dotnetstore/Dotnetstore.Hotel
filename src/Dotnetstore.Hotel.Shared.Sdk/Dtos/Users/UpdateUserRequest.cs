namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

public record UpdateUserRequest(string Email, string UserName, string Role, string? NewPassword);
