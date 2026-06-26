using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Microsoft.AspNetCore.Identity;

namespace Dotnetstore.Hotel.Api.Users.Features.CreateUser;

public class CreateUserCommandHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    public async Task<CreateUserResponse> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        if (!await roleManager.RoleExistsAsync(command.Role))
        {
            return new CreateUserResponse(null, [$"Role '{command.Role}' does not exist."]);
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            UserName = command.UserName,
            EmailConfirmed = true,
        };

        var createResult = await userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
        {
            return new CreateUserResponse(null, createResult.Errors.Select(e => e.Description).ToList());
        }

        var roleResult = await userManager.AddToRoleAsync(user, command.Role);
        if (!roleResult.Succeeded)
        {
            return new CreateUserResponse(null, roleResult.Errors.Select(e => e.Description).ToList());
        }

        return new CreateUserResponse(new UserDto(user.Id, command.Email, command.UserName, [command.Role]), []);
    }
}
