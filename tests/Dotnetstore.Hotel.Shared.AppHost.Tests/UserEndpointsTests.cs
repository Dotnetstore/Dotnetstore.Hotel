using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Shouldly;

namespace Dotnetstore.Hotel.Shared.AppHost.Tests.Tests;

public class UserEndpointsTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);

    // References Api.Users' actual SeedAdmin (via InternalsVisibleTo) instead of a copy-pasted literal,
    // so this can never silently drift out of sync with what Program.cs actually seeds.
    private static readonly Guid SeededAdminId = SeedAdmin.Id;
    private static readonly string SeededAdminEmail = SeedAdmin.Email;
    private static readonly string SeededAdminPassword = SeedAdmin.Password;

    [Fact]
    public async Task Login_CreateUser_GetUser_RoundTrips()
    {
        var cancellationToken = CancellationToken.None;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dotnetstore_Hotel_Shared_AppHost>(
            ["--IsIntegrationTest=true"], cancellationToken);
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        using var httpClient = app.CreateHttpClient("apiusers");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiusers", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var loginResponse = await httpClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(SeededAdminEmail, SeededAdminPassword),
            cancellationToken);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        login.ShouldNotBeNull();

        var uniqueSuffix = Guid.NewGuid().ToString("N");
        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/users")
        {
            Content = JsonContent.Create(new CreateUserRequest($"integration-desk-{uniqueSuffix}@dotnetstore.hotel", $"integration-desk-{uniqueSuffix}", "Desk1!2024", "desk")),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var createResponse = await httpClient.SendAsync(createRequest, cancellationToken);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateUserResponse>(cancellationToken);
        createResult.ShouldNotBeNull();
        var created = createResult.User;
        created.ShouldNotBeNull();
        created.Roles.ShouldBe(["desk"]);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{created.Id}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var getResponse = await httpClient.SendAsync(getRequest, cancellationToken);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<UserDto>(cancellationToken);
        fetched.ShouldNotBeNull();
        fetched.Email.ShouldBe($"integration-desk-{uniqueSuffix}@dotnetstore.hotel");

        using var unauthorizedRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{created.Id}");
        using var unauthorizedResponse = await httpClient.SendAsync(unauthorizedRequest, cancellationToken);
        unauthorizedResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_Rotate_Replay_Logout_RevokeAll_Flow()
    {
        var cancellationToken = CancellationToken.None;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dotnetstore_Hotel_Shared_AppHost>(
            ["--IsIntegrationTest=true"], cancellationToken);
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        using var httpClient = app.CreateHttpClient("apiusers");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiusers", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var firstLoginResponse = await httpClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(SeededAdminEmail, SeededAdminPassword),
            cancellationToken);
        firstLoginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var firstLogin = await firstLoginResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        firstLogin.ShouldNotBeNull();

        // Refresh: should rotate to a brand-new token pair.
        var refreshResponse = await httpClient.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(firstLogin.RefreshToken), cancellationToken);
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        refreshed.ShouldNotBeNull();
        refreshed.RefreshToken.ShouldNotBe(firstLogin.RefreshToken);

        // Replaying the now-rotated-out refresh token must be rejected.
        var replayResponse = await httpClient.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(firstLogin.RefreshToken), cancellationToken);
        replayResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // Logout revokes the current refresh token.
        var logoutResponse = await httpClient.PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequest(refreshed.RefreshToken), cancellationToken);
        logoutResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var refreshAfterLogoutResponse = await httpClient.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(refreshed.RefreshToken), cancellationToken);
        refreshAfterLogoutResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // A fresh login (separate session) followed by an admin revoke-all-for-user should invalidate it too.
        var secondLoginResponse = await httpClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(SeededAdminEmail, SeededAdminPassword),
            cancellationToken);
        var secondLogin = await secondLoginResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        secondLogin.ShouldNotBeNull();

        using var revokeAllRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/users/{SeededAdminId}/revoke-tokens");
        revokeAllRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secondLogin.Token);
        using var revokeAllResponse = await httpClient.SendAsync(revokeAllRequest, cancellationToken);
        revokeAllResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var revokeResult = await revokeAllResponse.Content.ReadFromJsonAsync<RevokeTokensResponse>(cancellationToken);
        revokeResult.ShouldNotBeNull();
        revokeResult.RevokedCount.ShouldBeGreaterThanOrEqualTo(1);

        var refreshAfterRevokeAllResponse = await httpClient.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(secondLogin.RefreshToken), cancellationToken);
        refreshAfterRevokeAllResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListUpdateDeactivateActivate_Flow()
    {
        var cancellationToken = CancellationToken.None;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dotnetstore_Hotel_Shared_AppHost>(
            ["--IsIntegrationTest=true"], cancellationToken);
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        using var httpClient = app.CreateHttpClient("apiusers");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiusers", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var adminLoginResponse = await httpClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(SeededAdminEmail, SeededAdminPassword),
            cancellationToken);
        var adminLogin = await adminLoginResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        adminLogin.ShouldNotBeNull();

        // Self-deactivation guard: admin cannot deactivate their own account.
        using var selfDeactivateRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{SeededAdminId}");
        selfDeactivateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var selfDeactivateResponse = await httpClient.SendAsync(selfDeactivateRequest, cancellationToken);
        selfDeactivateResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Create a throwaway user to drive through list -> update -> deactivate -> activate.
        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var email = $"crud-flow-{uniqueSuffix}@dotnetstore.hotel";
        var password = "Crud1Flow!2024";
        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/users")
        {
            Content = JsonContent.Create(new CreateUserRequest(email, $"crud-flow-{uniqueSuffix}", password, "desk")),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var createResponse = await httpClient.SendAsync(createRequest, cancellationToken);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateUserResponse>(cancellationToken);
        var created = createResult!.User;
        created.ShouldNotBeNull();

        // List: the new user should be present.
        using var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/users");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var listResponse = await httpClient.SendAsync(listRequest, cancellationToken);
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var users = await listResponse.Content.ReadFromJsonAsync<List<UserDto>>(cancellationToken);
        users.ShouldNotBeNull();
        users.ShouldContain(u => u.Id == created.Id);

        // Update: change username/role.
        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{created.Id}")
        {
            Content = JsonContent.Create(new UpdateUserRequest(email, $"crud-flow-renamed-{uniqueSuffix}", "restaurant", null)),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var updateResponse = await httpClient.SendAsync(updateRequest, cancellationToken);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updateResult = await updateResponse.Content.ReadFromJsonAsync<UpdateUserResponse>(cancellationToken);
        updateResult!.User.ShouldNotBeNull();
        updateResult.User.UserName.ShouldBe($"crud-flow-renamed-{uniqueSuffix}");
        updateResult.User.Roles.ShouldBe(["restaurant"]);

        // The deactivated user should no longer be able to log in.
        var preDeactivateLogin = await httpClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password), cancellationToken);
        preDeactivateLogin.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var deactivateRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{created.Id}");
        deactivateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var deactivateResponse = await httpClient.SendAsync(deactivateRequest, cancellationToken);
        deactivateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var postDeactivateLogin = await httpClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password), cancellationToken);
        postDeactivateLogin.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // Reactivating restores login access.
        using var activateRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/users/{created.Id}/activate");
        activateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var activateResponse = await httpClient.SendAsync(activateRequest, cancellationToken);
        activateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var postActivateLogin = await httpClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password), cancellationToken);
        postActivateLogin.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
