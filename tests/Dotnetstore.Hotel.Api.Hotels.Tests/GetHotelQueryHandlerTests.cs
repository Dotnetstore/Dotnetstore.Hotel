using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.GetHotel;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class GetHotelQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_HotelExists_ReturnsMappedDto()
    {
        var hotel = Domain.Hotel.Create(
            Guid.NewGuid(),
            "Test Hotel",
            new Address("Street 1", "City", "12345", "Country"),
            new ContactInfo("123", "test@hotel.com", null));
        hotel.SetAmenities(["Wifi", "Pool"]);

        var repository = new Mock<IHotelRepository>();
        repository.Setup(r => r.GetByIdAsync(hotel.Id, It.IsAny<CancellationToken>())).ReturnsAsync(hotel);

        var handler = new GetHotelQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new GetHotelQuery(hotel.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(hotel.Id);
        result.Name.ShouldBe("Test Hotel");
        result.Amenities.ShouldBe(["Wifi", "Pool"]);
    }

    [Fact]
    public async Task HandleAsync_HotelDoesNotExist_ReturnsNull()
    {
        var repository = new Mock<IHotelRepository>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Hotel?)null);

        var handler = new GetHotelQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new GetHotelQuery(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeNull();
    }
}
