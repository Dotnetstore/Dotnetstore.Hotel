using Dotnetstore.Hotel.Shared.Cqrs;

namespace Dotnetstore.Hotel.Api.Users.Features.Logout;

public record LogoutCommand(string RefreshToken) : ICommand<bool>;
