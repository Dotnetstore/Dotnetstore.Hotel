using Dotnetstore.Hotel.Shared.Cqrs;

namespace Dotnetstore.Hotel.Api.Users.Features.ActivateUser;

public record ActivateUserCommand(Guid Id) : ICommand<bool>;
