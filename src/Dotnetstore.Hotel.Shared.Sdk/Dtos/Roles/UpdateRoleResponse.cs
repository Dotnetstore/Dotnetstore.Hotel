namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;

public record UpdateRoleResponse(RoleDto? Role, IReadOnlyList<string> Errors);
