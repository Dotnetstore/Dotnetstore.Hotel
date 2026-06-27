using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;

namespace Dotnetstore.Hotel.Api.Users.Features.ListRoles;

public record ListRolesQuery : IQuery<IReadOnlyList<RoleDto>>;
