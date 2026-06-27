using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Microsoft.AspNetCore.Identity;

namespace Dotnetstore.Hotel.Api.Users.Features.UpdateUser;

public class UpdateUserCommandHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    public async Task<UpdateUserResponse> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.Id.ToString());
        if (user is null)
        {
            return new UpdateUserResponse(null, ["User not found."]);
        }

        if (!await roleManager.RoleExistsAsync(command.Role))
        {
            return new UpdateUserResponse(null, [$"Role '{command.Role}' does not exist."]);
        }

        var userNameResult = await userManager.SetUserNameAsync(user, command.UserName);
        if (!userNameResult.Succeeded)
        {
            return new UpdateUserResponse(null, userNameResult.Errors.Select(e => e.Description).ToList());
        }

        var emailResult = await userManager.SetEmailAsync(user, command.Email);
        if (!emailResult.Succeeded)
        {
            return new UpdateUserResponse(null, emailResult.Errors.Select(e => e.Description).ToList());
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return new UpdateUserResponse(null, removeResult.Errors.Select(e => e.Description).ToList());
            }
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, command.Role);
        if (!addRoleResult.Succeeded)
        {
            return new UpdateUserResponse(null, addRoleResult.Errors.Select(e => e.Description).ToList());
        }

        if (!string.IsNullOrWhiteSpace(command.NewPassword))
        {
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await userManager.ResetPasswordAsync(user, resetToken, command.NewPassword);
            if (!resetResult.Succeeded)
            {
                return new UpdateUserResponse(null, resetResult.Errors.Select(e => e.Description).ToList());
            }
        }

        var dto = await UserDtoMapper.ToDtoAsync(userManager, user, [command.Role]);
        return new UpdateUserResponse(dto, []);
    }
}
