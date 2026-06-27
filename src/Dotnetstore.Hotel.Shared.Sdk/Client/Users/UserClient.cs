using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

namespace Dotnetstore.Hotel.Shared.Sdk.Client.Users;

internal sealed class UserClient(HttpClient httpClient) : IUserClient
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/auth/login", request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
    }

    public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(refreshToken), cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequest(refreshToken), cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/users")
        {
            Content = JsonContent.Create(request),
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<CreateUserResponse>(cancellationToken);
        return result ?? new CreateUserResponse(null, ["Unexpected empty response"]);
    }

    public async Task<UserDto?> GetUserAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken);
    }

    public async Task<IReadOnlyList<UserDto>> ListUsersAsync(string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/users");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<UserDto>>(cancellationToken) ?? [];
    }

    public async Task<UpdateUserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{id}")
        {
            Content = JsonContent.Create(request),
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<UpdateUserResponse>(cancellationToken);
        return result ?? new UpdateUserResponse(null, ["Unexpected empty response"]);
    }

    public async Task<bool> DeactivateUserAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        return response.StatusCode == HttpStatusCode.NoContent;
    }

    public async Task<bool> ActivateUserAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/api/users/{id}/activate");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        return response.StatusCode == HttpStatusCode.NoContent;
    }

    public async Task<RevokeTokensResponse> RevokeUserTokensAsync(Guid userId, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/api/users/{userId}/revoke-tokens");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RevokeTokensResponse>(cancellationToken))!;
    }

    public async Task<IReadOnlyList<RoleDto>> ListRolesAsync(string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/roles");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<RoleDto>>(cancellationToken) ?? [];
    }

    public async Task<RoleDto?> GetRoleAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/roles/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RoleDto>(cancellationToken);
    }

    public async Task<CreateRoleResponse> CreateRoleAsync(CreateRoleRequest request, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/roles")
        {
            Content = JsonContent.Create(request),
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<CreateRoleResponse>(cancellationToken);
        return result ?? new CreateRoleResponse(null, ["Unexpected empty response"]);
    }

    public async Task<UpdateRoleResponse> UpdateRoleAsync(Guid id, UpdateRoleRequest request, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/roles/{id}")
        {
            Content = JsonContent.Create(request),
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<UpdateRoleResponse>(cancellationToken);
        return result ?? new UpdateRoleResponse(null, ["Unexpected empty response"]);
    }

    public async Task<DeleteRoleResponse> DeleteRoleAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/roles/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return new DeleteRoleResponse(true, []);
        }

        var result = await response.Content.ReadFromJsonAsync<DeleteRoleResponse>(cancellationToken);
        return result ?? new DeleteRoleResponse(false, ["Unexpected empty response"]);
    }
}
