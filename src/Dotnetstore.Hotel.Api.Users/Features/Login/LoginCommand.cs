using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

namespace Dotnetstore.Hotel.Api.Users.Features.Login;

public record LoginCommand(string Email, string Password) : ICommand<LoginResponse?>;
