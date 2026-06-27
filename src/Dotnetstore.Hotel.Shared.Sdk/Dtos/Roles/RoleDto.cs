namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;

public record RoleDto(Guid Id, string Name, int UserCount, bool IsProtected);
