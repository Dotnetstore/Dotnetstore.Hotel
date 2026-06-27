using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;

namespace Dotnetstore.Hotel.Api.Users.Features.GetRole;

public record GetRoleQuery(Guid Id) : IQuery<RoleDto?>;
