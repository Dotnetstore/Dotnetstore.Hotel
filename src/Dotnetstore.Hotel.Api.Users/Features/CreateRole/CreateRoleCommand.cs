using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;

namespace Dotnetstore.Hotel.Api.Users.Features.CreateRole;

public record CreateRoleCommand(string Name) : ICommand<CreateRoleResponse>;
