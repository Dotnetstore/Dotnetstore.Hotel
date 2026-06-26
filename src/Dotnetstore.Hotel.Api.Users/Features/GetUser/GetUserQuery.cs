using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

namespace Dotnetstore.Hotel.Api.Users.Features.GetUser;

public record GetUserQuery(Guid Id) : IQuery<UserDto?>;
