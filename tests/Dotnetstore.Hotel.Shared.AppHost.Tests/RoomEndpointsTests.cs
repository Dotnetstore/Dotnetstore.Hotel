using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Shouldly;

namespace Dotnetstore.Hotel.Shared.AppHost.Tests.Tests;

public class RoomEndpointsTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);

    // References Api.Users' actual SeedAdmin (via InternalsVisibleTo) instead of a copy-pasted literal,
    // so this can never silently drift out of sync with what Program.cs actually seeds.
    private static readonly string SeededAdminEmail = SeedAdmin.Email;
    private static readonly string SeededAdminPassword = SeedAdmin.Password;

    [Fact]
    public async Task CreateListGetUpdateDelete_Flow_And_AuthSplit()
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

        var adminLoginResponse = await usersHttpClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(SeededAdminEmail, SeededAdminPassword),
            cancellationToken);
        adminLoginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var adminLogin = await adminLoginResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        adminLogin.ShouldNotBeNull();

        // Anonymous (no token at all) must be rejected even for the "any authenticated user" List route.
        using var anonymousListResponse = await hotelsHttpClient.GetAsync("/api/rooms", cancellationToken);
        anonymousListResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var uniqueSuffix = Guid.NewGuid().ToString("N");

        using var createEquipmentRequest = new HttpRequestMessage(HttpMethod.Post, "/api/equipment")
        {
            Content = JsonContent.Create(new CreateEquipmentRequest($"Bed-{uniqueSuffix}", null)),
        };
        createEquipmentRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var createEquipmentResponse = await hotelsHttpClient.SendAsync(createEquipmentRequest, cancellationToken);
        createEquipmentResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var equipmentResult = await createEquipmentResponse.Content.ReadFromJsonAsync<CreateEquipmentResponse>(cancellationToken);
        var equipment = equipmentResult!.Equipment;
        equipment.ShouldNotBeNull();

        using var createTagRequest = new HttpRequestMessage(HttpMethod.Post, "/api/tags")
        {
            Content = JsonContent.Create(new CreateTagRequest($"Hard-{uniqueSuffix}")),
        };
        createTagRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var createTagResponse = await hotelsHttpClient.SendAsync(createTagRequest, cancellationToken);
        createTagResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var tagResult = await createTagResponse.Content.ReadFromJsonAsync<CreateTagResponse>(cancellationToken);
        var tag = tagResult!.Tag;
        tag.ShouldNotBeNull();

        var roomNumber = $"R-{uniqueSuffix}";
        using var createRoomRequest = new HttpRequestMessage(HttpMethod.Post, "/api/rooms")
        {
            Content = JsonContent.Create(new CreateRoomRequest(roomNumber, 3, 2, "Double", 175.50m, "Available", [new RoomEquipmentInput(equipment.Id, [tag.Id])])),
        };
        createRoomRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var createRoomResponse = await hotelsHttpClient.SendAsync(createRoomRequest, cancellationToken);
        createRoomResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createRoomResult = await createRoomResponse.Content.ReadFromJsonAsync<CreateRoomResponse>(cancellationToken);
        var createdRoom = createRoomResult!.Room;
        createdRoom.ShouldNotBeNull();
        createdRoom.RoomNumber.ShouldBe(roomNumber);
        var createdRoomEquipment = createdRoom.Equipment.Single(re => re.Equipment.Id == equipment.Id);
        createdRoomEquipment.Tags.ShouldContain(t => t.Id == tag.Id);

        // Create a non-admin ("desk") user to exercise the read/write authorization split.
        var deskEmail = $"room-test-desk-{uniqueSuffix}@dotnetstore.hotel";
        var deskPassword = "Desk1!2024";
        using var createDeskUserRequest = new HttpRequestMessage(HttpMethod.Post, "/api/users")
        {
            Content = JsonContent.Create(new CreateUserRequest(deskEmail, $"room-test-desk-{uniqueSuffix}", deskPassword, "desk")),
        };
        createDeskUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var createDeskUserResponse = await usersHttpClient.SendAsync(createDeskUserRequest, cancellationToken);
        createDeskUserResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var deskLoginResponse = await usersHttpClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(deskEmail, deskPassword), cancellationToken);
        deskLoginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var deskLogin = await deskLoginResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        deskLogin.ShouldNotBeNull();

        // A non-admin authenticated user can list/get rooms...
        using var deskListRequest = new HttpRequestMessage(HttpMethod.Get, "/api/rooms");
        deskListRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", deskLogin.Token);
        using var deskListResponse = await hotelsHttpClient.SendAsync(deskListRequest, cancellationToken);
        deskListResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var deskList = await deskListResponse.Content.ReadFromJsonAsync<List<RoomDto>>(cancellationToken);
        deskList.ShouldNotBeNull();
        deskList.ShouldContain(r => r.Id == createdRoom.Id);

        // ...but cannot create/update/delete - that stays admin-only (403, since the user is authenticated but lacks the role).
        using var deskCreateRequest = new HttpRequestMessage(HttpMethod.Post, "/api/rooms")
        {
            Content = JsonContent.Create(new CreateRoomRequest($"R2-{uniqueSuffix}", 1, 1, "Single", 50.00m, "Available", [])),
        };
        deskCreateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", deskLogin.Token);
        using var deskCreateResponse = await hotelsHttpClient.SendAsync(deskCreateRequest, cancellationToken);
        deskCreateResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/rooms/{createdRoom.Id}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var getResponse = await hotelsHttpClient.SendAsync(getRequest, cancellationToken);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/rooms/{createdRoom.Id}")
        {
            Content = JsonContent.Create(new UpdateRoomRequest(roomNumber, 4, 3, "Suite", 250.00m, "Maintenance", [])),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var updateResponse = await hotelsHttpClient.SendAsync(updateRequest, cancellationToken);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updateResult = await updateResponse.Content.ReadFromJsonAsync<UpdateRoomResponse>(cancellationToken);
        updateResult!.Room.ShouldNotBeNull();
        updateResult.Room.Status.ShouldBe("Maintenance");
        updateResult.Room.Equipment.ShouldBeEmpty();

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/rooms/{createdRoom.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var deleteResponse = await hotelsHttpClient.SendAsync(deleteRequest, cancellationToken);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var getAfterDeleteRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/rooms/{createdRoom.Id}");
        getAfterDeleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var getAfterDeleteResponse = await hotelsHttpClient.SendAsync(getAfterDeleteRequest, cancellationToken);
        getAfterDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
