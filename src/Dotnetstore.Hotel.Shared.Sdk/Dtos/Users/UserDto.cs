namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

public record UserDto(Guid Id, string Email, string UserName, IReadOnlyCollection<string> Roles, bool IsActive);
