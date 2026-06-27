using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

namespace Dotnetstore.Hotel.Api.Users.Features.UpdateUser;

public record UpdateUserCommand(Guid Id, string Email, string UserName, string Role, string? NewPassword) : ICommand<UpdateUserResponse>;
