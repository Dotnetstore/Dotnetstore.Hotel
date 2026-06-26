using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

namespace Dotnetstore.Hotel.Api.Users.Features.CreateUser;

public record CreateUserCommand(string Email, string UserName, string Password, string Role) : ICommand<CreateUserResponse>;
