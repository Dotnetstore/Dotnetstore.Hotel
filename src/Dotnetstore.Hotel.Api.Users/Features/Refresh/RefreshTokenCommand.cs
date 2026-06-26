using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

namespace Dotnetstore.Hotel.Api.Users.Features.Refresh;

public record RefreshTokenCommand(string RefreshToken) : ICommand<LoginResponse?>;
