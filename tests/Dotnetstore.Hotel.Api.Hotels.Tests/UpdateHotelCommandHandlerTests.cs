using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.UpdateHotel;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class UpdateHotelCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_HotelExists_UpdatesAndSaves()
    {
        var hotel = Domain.Hotel.Create(
            Guid.NewGuid(),
            "Old Name",
            new Address("Old Street", "Old City", "00000", "Old Country"),
            new ContactInfo("000", "old@hotel.com", null));

        var repository = new Mock<IHotelRepository>();
        repository.Setup(r => r.GetByIdAsync(hotel.Id, It.IsAny<CancellationToken>())).ReturnsAsync(hotel);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new UpdateHotelCommandHandler(repository.Object, unitOfWork.Object);
        var command = new UpdateHotelCommand(
            hotel.Id,
            "New Name",
            new AddressDto("New Street", "New City", "11111", "New Country"),
            new ContactInfoDto("111", "new@hotel.com", "https://new.hotel"),
            ["Spa"]);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("New Name");
        result.Address.City.ShouldBe("New City");
        result.Amenities.ShouldBe(["Spa"]);
        repository.Verify(r => r.Update(hotel), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_HotelDoesNotExist_ReturnsNullAndDoesNotSave()
    {
        var repository = new Mock<IHotelRepository>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Hotel?)null);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new UpdateHotelCommandHandler(repository.Object, unitOfWork.Object);
        var command = new UpdateHotelCommand(
            Guid.NewGuid(),
            "New Name",
            new AddressDto("Street", "City", "00000", "Country"),
            new ContactInfoDto("000", "x@y.com", null),
            []);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.ShouldBeNull();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
