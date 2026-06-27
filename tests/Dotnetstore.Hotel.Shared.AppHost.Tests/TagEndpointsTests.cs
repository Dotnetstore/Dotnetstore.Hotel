using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Shouldly;

namespace Dotnetstore.Hotel.Shared.AppHost.Tests.Tests;

public class TagEndpointsTests
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

        // A request without a bearer token must be rejected - Tags are AdminOnly, same as Equipment.
        using var unauthorizedCreateRequest = new HttpRequestMessage(HttpMethod.Post, "/api/tags")
        {
            Content = JsonContent.Create(new CreateTagRequest("Hard")),
        };
        using var unauthorizedCreateResponse = await hotelsHttpClient.SendAsync(unauthorizedCreateRequest, cancellationToken);
        unauthorizedCreateResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var name = $"Hard-{uniqueSuffix}";

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/tags")
        {
            Content = JsonContent.Create(new CreateTagRequest(name)),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var createResponse = await hotelsHttpClient.SendAsync(createRequest, cancellationToken);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateTagResponse>(cancellationToken);
        createResult.ShouldNotBeNull();
        var created = createResult.Tag;
        created.ShouldNotBeNull();
        created.Name.ShouldBe(name);

        using var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/tags");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var listResponse = await hotelsHttpClient.SendAsync(listRequest, cancellationToken);
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<List<TagDto>>(cancellationToken);
        list.ShouldNotBeNull();
        list.ShouldContain(t => t.Id == created.Id);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/tags/{created.Id}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var getResponse = await hotelsHttpClient.SendAsync(getRequest, cancellationToken);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/tags/{created.Id}")
        {
            Content = JsonContent.Create(new UpdateTagRequest($"{name}-Renamed")),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var updateResponse = await hotelsHttpClient.SendAsync(updateRequest, cancellationToken);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updateResult = await updateResponse.Content.ReadFromJsonAsync<UpdateTagResponse>(cancellationToken);
        updateResult!.Tag.ShouldNotBeNull();
        updateResult.Tag.Name.ShouldBe($"{name}-Renamed");

        // In-use guard: a room-equipment entry referencing this tag must block deletion, naming the count.
        using var createEquipmentRequest = new HttpRequestMessage(HttpMethod.Post, "/api/equipment")
        {
            Content = JsonContent.Create(new CreateEquipmentRequest($"Bed-{uniqueSuffix}", null)),
        };
        createEquipmentRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var createEquipmentResponse = await hotelsHttpClient.SendAsync(createEquipmentRequest, cancellationToken);
        createEquipmentResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var equipmentResult = await createEquipmentResponse.Content.ReadFromJsonAsync<CreateEquipmentResponse>(cancellationToken);
        var equipment = equipmentResult!.Equipment;
        equipment.ShouldNotBeNull();

        using var createRoomRequest = new HttpRequestMessage(HttpMethod.Post, "/api/rooms")
        {
            Content = JsonContent.Create(new CreateRoomRequest($"TAG-TEST-{uniqueSuffix}", 1, 2, "Double", 100.00m, "Available", [new RoomEquipmentInput(equipment.Id, [created.Id])])),
        };
        createRoomRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var createRoomResponse = await hotelsHttpClient.SendAsync(createRoomRequest, cancellationToken);
        createRoomResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createRoomResult = await createRoomResponse.Content.ReadFromJsonAsync<CreateRoomResponse>(cancellationToken);
        var createdRoom = createRoomResult!.Room;
        createdRoom.ShouldNotBeNull();

        using var blockedDeleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/tags/{created.Id}");
        blockedDeleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var blockedDeleteResponse = await hotelsHttpClient.SendAsync(blockedDeleteRequest, cancellationToken);
        blockedDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var blockedResult = await blockedDeleteResponse.Content.ReadFromJsonAsync<DeleteTagResponse>(cancellationToken);
        blockedResult!.Errors.ShouldContain(e => e.Contains('1'));

        using var deleteRoomRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/rooms/{createdRoom.Id}");
        deleteRoomRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var deleteRoomResponse = await hotelsHttpClient.SendAsync(deleteRoomRequest, cancellationToken);
        deleteRoomResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/tags/{created.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var deleteResponse = await hotelsHttpClient.SendAsync(deleteRequest, cancellationToken);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var getAfterDeleteRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/tags/{created.Id}");
        getAfterDeleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        using var getAfterDeleteResponse = await hotelsHttpClient.SendAsync(getAfterDeleteRequest, cancellationToken);
        getAfterDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
