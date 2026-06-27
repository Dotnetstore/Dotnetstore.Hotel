using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

namespace Dotnetstore.Hotel.Shared.Sdk.Client.Users;

public interface IUserClient
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<LoginResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<UserDto?> GetUserAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDto>> ListUsersAsync(string bearerToken, CancellationToken cancellationToken = default);

    Task<UpdateUserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<bool> DeactivateUserAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<bool> ActivateUserAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<RevokeTokensResponse> RevokeUserTokensAsync(Guid userId, string bearerToken, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoleDto>> ListRolesAsync(string bearerToken, CancellationToken cancellationToken = default);

    Task<RoleDto?> GetRoleAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<CreateRoleResponse> CreateRoleAsync(CreateRoleRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<UpdateRoleResponse> UpdateRoleAsync(Guid id, UpdateRoleRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<DeleteRoleResponse> DeleteRoleAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);
}
