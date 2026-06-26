using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

namespace Dotnetstore.Hotel.Shared.Sdk.Client.Users;

public interface IUserClient
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<LoginResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<UserDto?> GetUserAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<RevokeTokensResponse> RevokeUserTokensAsync(Guid userId, string bearerToken, CancellationToken cancellationToken = default);
}
