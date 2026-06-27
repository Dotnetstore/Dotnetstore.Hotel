using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;

namespace Dotnetstore.Hotel.Api.Users.Features.UpdateRole;

public record UpdateRoleCommand(Guid Id, string Name) : ICommand<UpdateRoleResponse>;
