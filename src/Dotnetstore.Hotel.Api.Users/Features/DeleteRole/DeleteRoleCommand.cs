using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;

namespace Dotnetstore.Hotel.Api.Users.Features.DeleteRole;

public record DeleteRoleCommand(Guid Id) : ICommand<DeleteRoleResponse>;
