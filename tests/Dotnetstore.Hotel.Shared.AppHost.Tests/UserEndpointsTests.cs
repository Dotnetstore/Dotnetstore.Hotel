using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Shouldly;

namespace Dotnetstore.Hotel.Shared.AppHost.Tests.Tests;

public class UserEndpointsTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);
    private static readonly Guid SeededAdminId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    [Fact]
    public async Task Login_CreateUser_GetUser_RoundTrips()
    {
        var cancellationToken = CancellationToken.None;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dotnetstore_Hotel_Shared_AppHost>(cancellationToken);
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
            new LoginRequest("admin@dotnetstore.hotel", "Adm1n!2024"),
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
        var created = await createResponse.Content.ReadFromJsonAsync<UserDto>(cancellationToken);
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
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dotnetstore_Hotel_Shared_AppHost>(cancellationToken);
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
            new LoginRequest("admin@dotnetstore.hotel", "Adm1n!2024"),
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
            new LoginRequest("admin@dotnetstore.hotel", "Adm1n!2024"),
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
}
