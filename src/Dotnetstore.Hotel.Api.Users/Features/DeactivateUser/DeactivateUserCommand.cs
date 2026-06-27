using Dotnetstore.Hotel.Shared.Cqrs;

namespace Dotnetstore.Hotel.Api.Users.Features.DeactivateUser;

public record DeactivateUserCommand(Guid Id, Guid RequestedByUserId) : ICommand<bool>;
