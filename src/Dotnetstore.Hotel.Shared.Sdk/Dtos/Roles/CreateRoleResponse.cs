namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;

public record CreateRoleResponse(RoleDto? Role, IReadOnlyList<string> Errors);
