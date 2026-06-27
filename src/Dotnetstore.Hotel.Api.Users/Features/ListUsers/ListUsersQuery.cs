using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

namespace Dotnetstore.Hotel.Api.Users.Features.ListUsers;

public record ListUsersQuery : IQuery<IReadOnlyList<UserDto>>;
