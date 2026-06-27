using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Shouldly;

namespace Dotnetstore.Hotel.Shared.AppHost.Tests.Tests;

public class RoleEndpointsTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);

    [Fact]
    public async Task CreateListGetUpdateDelete_Flow_And_Guards()
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
            new LoginRequest(SeedAdmin.Email, SeedAdmin.Password),
            cancellationToken);
        var adminLogin = await adminLoginResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        adminLogin.ShouldNotBeNull();
        var bearer = new AuthenticationHeaderValue("Bearer", adminLogin.Token);

        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var roleName = $"housekeeping-{uniqueSuffix}";

        // Create.
        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/roles")
        {
            Content = JsonContent.Create(new CreateRoleRequest(roleName)),
        };
        createRequest.Headers.Authorization = bearer;
        using var createResponse = await httpClient.SendAsync(createRequest, cancellationToken);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateRoleResponse>(cancellationToken);
        var created = createResult!.Role;
        created.ShouldNotBeNull();
        created.UserCount.ShouldBe(0);
        created.IsProtected.ShouldBeFalse();

        // List: the new role should be present.
        using var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/roles");
        listRequest.Headers.Authorization = bearer;
        using var listResponse = await httpClient.SendAsync(listRequest, cancellationToken);
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var roles = await listResponse.Content.ReadFromJsonAsync<List<RoleDto>>(cancellationToken);
        roles.ShouldNotBeNull();
        roles.ShouldContain(r => r.Id == created.Id);

        // Get by id.
        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/roles/{created.Id}");
        getRequest.Headers.Authorization = bearer;
        using var getResponse = await httpClient.SendAsync(getRequest, cancellationToken);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<RoleDto>(cancellationToken);
        fetched.ShouldNotBeNull();
        fetched.Name.ShouldBe(roleName);

        // Rename.
        var renamedName = $"housekeeping-renamed-{uniqueSuffix}";
        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/roles/{created.Id}")
        {
            Content = JsonContent.Create(new UpdateRoleRequest(renamedName)),
        };
        updateRequest.Headers.Authorization = bearer;
        using var updateResponse = await httpClient.SendAsync(updateRequest, cancellationToken);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updateResult = await updateResponse.Content.ReadFromJsonAsync<UpdateRoleResponse>(cancellationToken);
        updateResult!.Role.ShouldNotBeNull();
        updateResult.Role.Name.ShouldBe(renamedName);

        // Delete while unused: succeeds.
        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/roles/{created.Id}");
        deleteRequest.Headers.Authorization = bearer;
        using var deleteResponse = await httpClient.SendAsync(deleteRequest, cancellationToken);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Protected-role guard: deleting "administrator" is rejected.
        var adminRoleId = roles!.Single(r => r.Name == "administrator").Id;
        using var deleteAdminRoleRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/roles/{adminRoleId}");
        deleteAdminRoleRequest.Headers.Authorization = bearer;
        using var deleteAdminRoleResponse = await httpClient.SendAsync(deleteAdminRoleRequest, cancellationToken);
        deleteAdminRoleResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Renaming "administrator" is also rejected.
        using var renameAdminRoleRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/roles/{adminRoleId}")
        {
            Content = JsonContent.Create(new UpdateRoleRequest("not-administrator")),
        };
        renameAdminRoleRequest.Headers.Authorization = bearer;
        using var renameAdminRoleResponse = await httpClient.SendAsync(renameAdminRoleRequest, cancellationToken);
        renameAdminRoleResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // In-use guard: a role assigned to a user cannot be deleted until reassigned.
        var inUseRoleName = $"temp-role-{uniqueSuffix}";
        using var createInUseRoleRequest = new HttpRequestMessage(HttpMethod.Post, "/api/roles")
        {
            Content = JsonContent.Create(new CreateRoleRequest(inUseRoleName)),
        };
        createInUseRoleRequest.Headers.Authorization = bearer;
        using var createInUseRoleResponse = await httpClient.SendAsync(createInUseRoleRequest, cancellationToken);
        var inUseRole = (await createInUseRoleResponse.Content.ReadFromJsonAsync<CreateRoleResponse>(cancellationToken))!.Role;
        inUseRole.ShouldNotBeNull();

        using var createUserRequest = new HttpRequestMessage(HttpMethod.Post, "/api/users")
        {
            Content = JsonContent.Create(new CreateUserRequest($"role-flow-{uniqueSuffix}@dotnetstore.hotel", $"role-flow-{uniqueSuffix}", "RoleFlow1!2024", inUseRoleName)),
        };
        createUserRequest.Headers.Authorization = bearer;
        using var createUserResponse = await httpClient.SendAsync(createUserRequest, cancellationToken);
        var createdUser = (await createUserResponse.Content.ReadFromJsonAsync<CreateUserResponse>(cancellationToken))!.User;
        createdUser.ShouldNotBeNull();

        using var deleteInUseRoleRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/roles/{inUseRole.Id}");
        deleteInUseRoleRequest.Headers.Authorization = bearer;
        using var deleteInUseRoleResponse = await httpClient.SendAsync(deleteInUseRoleRequest, cancellationToken);
        deleteInUseRoleResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var deleteInUseResult = await deleteInUseRoleResponse.Content.ReadFromJsonAsync<DeleteRoleResponse>(cancellationToken);
        deleteInUseResult!.Errors.Single().ShouldContain("1");

        // Reassign the user away, then deletion succeeds.
        using var reassignRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{createdUser.Id}")
        {
            Content = JsonContent.Create(new UpdateUserRequest(createdUser.Email, createdUser.UserName, "desk", null)),
        };
        reassignRequest.Headers.Authorization = bearer;
        using var reassignResponse = await httpClient.SendAsync(reassignRequest, cancellationToken);
        reassignResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var deleteAfterReassignRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/roles/{inUseRole.Id}");
        deleteAfterReassignRequest.Headers.Authorization = bearer;
        using var deleteAfterReassignResponse = await httpClient.SendAsync(deleteAfterReassignRequest, cancellationToken);
        deleteAfterReassignResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
