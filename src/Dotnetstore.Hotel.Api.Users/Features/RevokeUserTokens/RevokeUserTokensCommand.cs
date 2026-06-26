using Dotnetstore.Hotel.Shared.Cqrs;

namespace Dotnetstore.Hotel.Api.Users.Features.RevokeUserTokens;

public record RevokeUserTokensCommand(Guid UserId) : ICommand<int>;
