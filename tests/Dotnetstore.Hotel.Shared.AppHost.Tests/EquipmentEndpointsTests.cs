using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Shouldly;

namespace Dotnetstore.Hotel.Shared.AppHost.Tests.Tests;

public class EquipmentEndpointsTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);

    // References Api.Users' actual SeedAdmin (via InternalsVisibleTo) instead of a copy-pasted literal,
    // so this can never silently drift out of sync with what Program.cs actually seeds.
    private static readonly string SeededAdminEmail = SeedAdmin.Email;
    private static readonly string SeededAdminPassword = SeedAdmin.Password;

    [Fact]
    public async Task CreateListGetUpdateDelete_Flow_And_AuthGuard()
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

        using var usersHttpClient = app.CreateHttpClient("apiusers");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiusers", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        using var hotelsHttpClient = app.CreateHttpClient("apihotels");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apihotels", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var loginResponse = await usersHttpClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(SeededAdminEmail, SeededAdminPassword),
            cancellationToken);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        login.ShouldNotBeNull();

        // A request without a bearer token must be rejected - this is the new AdminOnly guard on Api.Hotels.
        using var unauthorizedCreateRequest = new HttpRequestMessage(HttpMethod.Post, "/api/equipment")
        {
            Content = JsonContent.Create(new CreateEquipmentRequest("Bed", null)),
        };
        using var unauthorizedCreateResponse = await hotelsHttpClient.SendAsync(unauthorizedCreateRequest, cancellationToken);
        unauthorizedCreateResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var name = $"TV-{uniqueSuffix}";

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/equipment")
        {
            Content = JsonContent.Create(new CreateEquipmentRequest(name, "55-inch flatscreen")),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var createResponse = await hotelsHttpClient.SendAsync(createRequest, cancellationToken);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateEquipmentResponse>(cancellationToken);
        createResult.ShouldNotBeNull();
        var created = createResult.Equipment;
        created.ShouldNotBeNull();
        created.Name.ShouldBe(name);

        using var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/equipment");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var listResponse = await hotelsHttpClient.SendAsync(listRequest, cancellationToken);
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<List<EquipmentDto>>(cancellationToken);
        list.ShouldNotBeNull();
        list.ShouldContain(e => e.Id == created.Id);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/equipment/{created.Id}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var getResponse = await hotelsHttpClient.SendAsync(getRequest, cancellationToken);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/equipment/{created.Id}")
        {
            Content = JsonContent.Create(new UpdateEquipmentRequest($"{name}-Renamed", "Updated description")),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var updateResponse = await hotelsHttpClient.SendAsync(updateRequest, cancellationToken);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updateResult = await updateResponse.Content.ReadFromJsonAsync<UpdateEquipmentResponse>(cancellationToken);
        updateResult!.Equipment.ShouldNotBeNull();
        updateResult.Equipment.Name.ShouldBe($"{name}-Renamed");

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/equipment/{created.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var deleteResponse = await hotelsHttpClient.SendAsync(deleteRequest, cancellationToken);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var getAfterDeleteRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/equipment/{created.Id}");
        getAfterDeleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var getAfterDeleteResponse = await hotelsHttpClient.SendAsync(getAfterDeleteRequest, cancellationToken);
        getAfterDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
