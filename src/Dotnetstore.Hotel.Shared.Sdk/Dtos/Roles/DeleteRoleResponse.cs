namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;

public record DeleteRoleResponse(bool Succeeded, IReadOnlyList<string> Errors);
