using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Shouldly;

namespace Dotnetstore.Hotel.Shared.AppHost.Tests.Tests;

public class CustomerEndpointsTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);

    // References Api.Users' actual SeedAdmin (via InternalsVisibleTo) instead of a copy-pasted literal,
    // so this can never silently drift out of sync with what Program.cs actually seeds.
    private static readonly string SeededAdminEmail = SeedAdmin.Email;
    private static readonly string SeededAdminPassword = SeedAdmin.Password;

    [Fact]
    public async Task CreateListGetUpdateDelete_Flow_And_FrontDeskOnlyGuard()
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

        var uniqueSuffix = Guid.NewGuid().ToString("N");

        // A request without a bearer token must be rejected.
        var address = new AddressDto("Street 1", "City", "12345", "Country");
        using var unauthorizedCreateRequest = new HttpRequestMessage(HttpMethod.Post, "/api/customers")
        {
            Content = JsonContent.Create(new CreateCustomerRequest("Alice", "Passport", $"P-{uniqueSuffix}", address, "111", "alice@x.com", new DateOnly(1990, 1, 1), "Swedish", null)),
        };
        using var unauthorizedCreateResponse = await hotelsHttpClient.SendAsync(unauthorizedCreateRequest, cancellationToken);
        unauthorizedCreateResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // A "desk" user (not admin) can manage customers - that's the point of the FrontDeskOnly policy.
        var deskEmail = $"customer-test-desk-{uniqueSuffix}@dotnetstore.hotel";
        var deskPassword = "Desk1!2024";
        using var createDeskUserRequest = new HttpRequestMessage(HttpMethod.Post, "/api/users")
        {
            Content = JsonContent.Create(new CreateUserRequest(deskEmail, $"customer-test-desk-{uniqueSuffix}", deskPassword, "desk")),
        };
        createDeskUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var createDeskUserResponse = await usersHttpClient.SendAsync(createDeskUserRequest, cancellationToken);
        createDeskUserResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var deskLoginResponse = await usersHttpClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(deskEmail, deskPassword), cancellationToken);
        deskLoginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var deskLogin = await deskLoginResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        deskLogin.ShouldNotBeNull();

        var identificationNumber = $"P-{uniqueSuffix}";
        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/customers")
        {
            Content = JsonContent.Create(new CreateCustomerRequest("Alice Smith", "Passport", identificationNumber, address, "111-222", "alice@x.com", new DateOnly(1990, 1, 1), "Swedish", "First visit")),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", deskLogin.Token);
        using var createResponse = await hotelsHttpClient.SendAsync(createRequest, cancellationToken);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateCustomerResponse>(cancellationToken);
        var created = createResult!.Customer;
        created.ShouldNotBeNull();
        created.FullName.ShouldBe("Alice Smith");
        created.CustomerNumber.ShouldBeGreaterThan(0);

        // A user with a role outside {administrator, superuser, desk} must be rejected (403, authenticated but lacking the role).
        var restaurantEmail = $"customer-test-restaurant-{uniqueSuffix}@dotnetstore.hotel";
        var restaurantPassword = "Resto1!2024";
        using var createRestaurantUserRequest = new HttpRequestMessage(HttpMethod.Post, "/api/users")
        {
            Content = JsonContent.Create(new CreateUserRequest(restaurantEmail, $"customer-test-restaurant-{uniqueSuffix}", restaurantPassword, "restaurant")),
        };
        createRestaurantUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var createRestaurantUserResponse = await usersHttpClient.SendAsync(createRestaurantUserRequest, cancellationToken);
        createRestaurantUserResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var restaurantLoginResponse = await usersHttpClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(restaurantEmail, restaurantPassword), cancellationToken);
        var restaurantLogin = await restaurantLoginResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        restaurantLogin.ShouldNotBeNull();

        using var restaurantListRequest = new HttpRequestMessage(HttpMethod.Get, "/api/customers");
        restaurantListRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", restaurantLogin.Token);
        using var restaurantListResponse = await hotelsHttpClient.SendAsync(restaurantListRequest, cancellationToken);
        restaurantListResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // List with a search term matching the identification number finds the customer.
        using var listRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/customers?search={identificationNumber}");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", deskLogin.Token);
        using var listResponse = await hotelsHttpClient.SendAsync(listRequest, cancellationToken);
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<List<CustomerDto>>(cancellationToken);
        list.ShouldNotBeNull();
        list.ShouldContain(c => c.Id == created.Id);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/customers/{created.Id}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", deskLogin.Token);
        using var getResponse = await hotelsHttpClient.SendAsync(getRequest, cancellationToken);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/customers/{created.Id}")
        {
            Content = JsonContent.Create(new UpdateCustomerRequest("Alice Johnson", "Passport", identificationNumber, address, "999-888", "ajohnson@x.com", new DateOnly(1990, 1, 1), "Swedish", "Returning guest")),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", deskLogin.Token);
        using var updateResponse = await hotelsHttpClient.SendAsync(updateRequest, cancellationToken);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updateResult = await updateResponse.Content.ReadFromJsonAsync<UpdateCustomerResponse>(cancellationToken);
        updateResult!.Customer.ShouldNotBeNull();
        updateResult.Customer.FullName.ShouldBe("Alice Johnson");
        updateResult.Customer.Notes.ShouldBe("Returning guest");

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/customers/{created.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", deskLogin.Token);
        using var deleteResponse = await hotelsHttpClient.SendAsync(deleteRequest, cancellationToken);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var getAfterDeleteRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/customers/{created.Id}");
        getAfterDeleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", deskLogin.Token);
        using var getAfterDeleteResponse = await hotelsHttpClient.SendAsync(getAfterDeleteRequest, cancellationToken);
        getAfterDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
