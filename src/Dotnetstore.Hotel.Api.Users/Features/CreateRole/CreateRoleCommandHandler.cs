using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;
using Microsoft.AspNetCore.Identity;

namespace Dotnetstore.Hotel.Api.Users.Features.CreateRole;

public class CreateRoleCommandHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    : ICommandHandler<CreateRoleCommand, CreateRoleResponse>
{
    public async Task<CreateRoleResponse> HandleAsync(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return new CreateRoleResponse(null, ["Role name is required."]);
        }

        if (await roleManager.RoleExistsAsync(command.Name))
        {
            return new CreateRoleResponse(null, [$"Role '{command.Name}' already exists."]);
        }

        var role = new ApplicationRole { Id = Guid.NewGuid(), Name = command.Name };
        var createResult = await roleManager.CreateAsync(role);
        if (!createResult.Succeeded)
        {
            return new CreateRoleResponse(null, createResult.Errors.Select(e => e.Description).ToList());
        }

        var dto = await RoleDtoMapper.ToDtoAsync(userManager, role);
        return new CreateRoleResponse(dto, []);
    }
}
