using System.Net.Http.Json;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;
using Shouldly;

namespace Dotnetstore.Hotel.Shared.AppHost.Tests.Tests;

public class HotelEndpointsTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);
    private static readonly Guid SeededHotelId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task GetThenUpdateHotel_RoundTrips()
    {
        var cancellationToken = CancellationToken.None;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Dotnetstore_Hotel_Shared_AppHost>(cancellationToken);
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        using var httpClient = app.CreateHttpClient("apihotels");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apihotels", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var seeded = await httpClient.GetFromJsonAsync<HotelDto>($"/api/hotels/{SeededHotelId}", cancellationToken);
        seeded.ShouldNotBeNull();

        var updateRequest = new UpdateHotelRequest("Updated Name", seeded.Address, seeded.ContactInfo, ["Wifi", "Spa"]);
        using var updateResponse = await httpClient.PutAsJsonAsync($"/api/hotels/{seeded.Id}", updateRequest, cancellationToken);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<HotelDto>(cancellationToken);
        updated.ShouldNotBeNull();
        updated.Name.ShouldBe("Updated Name");
        updated.Amenities.ShouldBe(["Wifi", "Spa"]);
    }
}
