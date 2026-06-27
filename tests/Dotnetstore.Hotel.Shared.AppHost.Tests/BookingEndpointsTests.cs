using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Shouldly;

namespace Dotnetstore.Hotel.Shared.AppHost.Tests.Tests;

public class BookingEndpointsTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);

    private static readonly string SeededAdminEmail = SeedAdmin.Email;
    private static readonly string SeededAdminPassword = SeedAdmin.Password;

    [Fact]
    public async Task CreateBooking_BlocksOverlappingAvailability_AndDrivesLifecycle()
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
        var adminLogin = await adminLoginResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        adminLogin.ShouldNotBeNull();

        var uniqueSuffix = Guid.NewGuid().ToString("N");

        // Create a customer.
        var address = new AddressDto("Street 1", "City", "12345", "Country");
        using var createCustomerRequest = new HttpRequestMessage(HttpMethod.Post, "/api/customers")
        {
            Content = JsonContent.Create(new CreateCustomerRequest("Bob Booker", "Passport", $"P-{uniqueSuffix}", address, "555", "bob@x.com", new DateOnly(1985, 5, 5), "Danish", null)),
        };
        createCustomerRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var createCustomerResponse = await hotelsHttpClient.SendAsync(createCustomerRequest, cancellationToken);
        createCustomerResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var customerResult = await createCustomerResponse.Content.ReadFromJsonAsync<CreateCustomerResponse>(cancellationToken);
        var customer = customerResult!.Customer;
        customer.ShouldNotBeNull();

        // Create a room.
        var roomNumber = $"B-{uniqueSuffix}";
        using var createRoomRequest = new HttpRequestMessage(HttpMethod.Post, "/api/rooms")
        {
            Content = JsonContent.Create(new CreateRoomRequest(roomNumber, 1, 2, "Double", 120.00m, "Available", [])),
        };
        createRoomRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var createRoomResponse = await hotelsHttpClient.SendAsync(createRoomRequest, cancellationToken);
        createRoomResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var roomResult = await createRoomResponse.Content.ReadFromJsonAsync<CreateRoomResponse>(cancellationToken);
        var room = roomResult!.Room;
        room.ShouldNotBeNull();

        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
        var checkOut = checkIn.AddDays(3);

        // Before booking, the room shows up in an availability search for these dates.
        using var beforeSearchRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/rooms/search?checkInDate={checkIn:yyyy-MM-dd}&checkOutDate={checkOut:yyyy-MM-dd}");
        beforeSearchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var beforeSearchResponse = await hotelsHttpClient.SendAsync(beforeSearchRequest, cancellationToken);
        beforeSearchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var beforeSearch = await beforeSearchResponse.Content.ReadFromJsonAsync<List<RoomDto>>(cancellationToken);
        beforeSearch.ShouldNotBeNull();
        beforeSearch.ShouldContain(r => r.Id == room.Id);

        // Create the booking.
        using var createBookingRequest = new HttpRequestMessage(HttpMethod.Post, "/api/bookings")
        {
            Content = JsonContent.Create(new CreateBookingRequest(customer.Id, checkIn, checkOut, [room.Id])),
        };
        createBookingRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var createBookingResponse = await hotelsHttpClient.SendAsync(createBookingRequest, cancellationToken);
        createBookingResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var bookingResult = await createBookingResponse.Content.ReadFromJsonAsync<CreateBookingResponse>(cancellationToken);
        var booking = bookingResult!.Booking;
        booking.ShouldNotBeNull();
        booking.Status.ShouldBe("Reserved");
        booking.Customer.Id.ShouldBe(customer.Id);
        booking.Rooms.ShouldContain(r => r.Id == room.Id);

        // An overlapping date range no longer finds the room.
        var overlappingCheckIn = checkIn.AddDays(1);
        var overlappingCheckOut = checkOut.AddDays(1);
        using var afterSearchRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/rooms/search?checkInDate={overlappingCheckIn:yyyy-MM-dd}&checkOutDate={overlappingCheckOut:yyyy-MM-dd}");
        afterSearchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var afterSearchResponse = await hotelsHttpClient.SendAsync(afterSearchRequest, cancellationToken);
        afterSearchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var afterSearch = await afterSearchResponse.Content.ReadFromJsonAsync<List<RoomDto>>(cancellationToken);
        afterSearch.ShouldNotBeNull();
        afterSearch.ShouldNotContain(r => r.Id == room.Id);

        // A second booking attempt for the same room/overlapping dates is rejected outright.
        using var conflictingBookingRequest = new HttpRequestMessage(HttpMethod.Post, "/api/bookings")
        {
            Content = JsonContent.Create(new CreateBookingRequest(customer.Id, overlappingCheckIn, overlappingCheckOut, [room.Id])),
        };
        conflictingBookingRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var conflictingBookingResponse = await hotelsHttpClient.SendAsync(conflictingBookingRequest, cancellationToken);
        conflictingBookingResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Get by id.
        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/bookings/{booking.Id}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var getResponse = await hotelsHttpClient.SendAsync(getRequest, cancellationToken);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // List filtered by customer.
        using var listRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/bookings?customerId={customer.Id}");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var listResponse = await hotelsHttpClient.SendAsync(listRequest, cancellationToken);
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<List<BookingDto>>(cancellationToken);
        list.ShouldNotBeNull();
        list.ShouldContain(b => b.Id == booking.Id);

        // Check-in, then check-out.
        using var checkInRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/bookings/{booking.Id}/check-in");
        checkInRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var checkInResponse = await hotelsHttpClient.SendAsync(checkInRequest, cancellationToken);
        checkInResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Cannot cancel a checked-in booking is allowed (still active) - but cannot check-in again.
        using var doubleCheckInRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/bookings/{booking.Id}/check-in");
        doubleCheckInRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var doubleCheckInResponse = await hotelsHttpClient.SendAsync(doubleCheckInRequest, cancellationToken);
        doubleCheckInResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        using var checkOutRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/bookings/{booking.Id}/check-out");
        checkOutRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var checkOutResponse = await hotelsHttpClient.SendAsync(checkOutRequest, cancellationToken);
        checkOutResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // A checked-out booking can no longer be cancelled.
        using var cancelAfterCheckOutRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/bookings/{booking.Id}/cancel");
        cancelAfterCheckOutRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var cancelAfterCheckOutResponse = await hotelsHttpClient.SendAsync(cancelAfterCheckOutRequest, cancellationToken);
        cancelAfterCheckOutResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Separate cancel flow: a fresh reserved booking on a different room can be cancelled outright.
        var roomNumber2 = $"B2-{uniqueSuffix}";
        using var createRoom2Request = new HttpRequestMessage(HttpMethod.Post, "/api/rooms")
        {
            Content = JsonContent.Create(new CreateRoomRequest(roomNumber2, 1, 2, "Double", 120.00m, "Available", [])),
        };
        createRoom2Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var createRoom2Response = await hotelsHttpClient.SendAsync(createRoom2Request, cancellationToken);
        var room2Result = await createRoom2Response.Content.ReadFromJsonAsync<CreateRoomResponse>(cancellationToken);
        var room2 = room2Result!.Room;
        room2.ShouldNotBeNull();

        using var createBooking2Request = new HttpRequestMessage(HttpMethod.Post, "/api/bookings")
        {
            Content = JsonContent.Create(new CreateBookingRequest(customer.Id, checkIn, checkOut, [room2.Id])),
        };
        createBooking2Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var createBooking2Response = await hotelsHttpClient.SendAsync(createBooking2Request, cancellationToken);
        var booking2Result = await createBooking2Response.Content.ReadFromJsonAsync<CreateBookingResponse>(cancellationToken);
        var booking2 = booking2Result!.Booking;
        booking2.ShouldNotBeNull();

        using var cancelRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/bookings/{booking2.Id}/cancel");
        cancelRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var cancelResponse = await hotelsHttpClient.SendAsync(cancelRequest, cancellationToken);
        cancelResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var cancelResult = await cancelResponse.Content.ReadFromJsonAsync<BookingActionResponse>(cancellationToken);
        cancelResult!.Succeeded.ShouldBeTrue();

        // A cancelled booking frees the room back up for the same dates.
        using var afterCancelSearchRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/rooms/search?checkInDate={checkIn:yyyy-MM-dd}&checkOutDate={checkOut:yyyy-MM-dd}");
        afterCancelSearchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        using var afterCancelSearchResponse = await hotelsHttpClient.SendAsync(afterCancelSearchRequest, cancellationToken);
        var afterCancelSearch = await afterCancelSearchResponse.Content.ReadFromJsonAsync<List<RoomDto>>(cancellationToken);
        afterCancelSearch.ShouldNotBeNull();
        afterCancelSearch.ShouldContain(r => r.Id == room2.Id);
    }
}
