using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.UpdateCustomer;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;
using Moq;
using Shouldly;
using CustomerEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class UpdateCustomerCommandHandlerTests
{
    private static readonly AddressDto ValidAddress = new("Street 1", "City", "12345", "Country");

    [Fact]
    public async Task HandleAsync_CustomerExists_UpdatesAndSaves()
    {
        var customer = CustomerEntity.Create(Guid.NewGuid(), "Alice", IdentificationTypes.Passport, "P1", new Address("S", "C", "Z", "Co"), "111", "alice@x.com", new DateOnly(1990, 1, 1), "Swedish", null);

        var repository = new Mock<ICustomerRepository>();
        repository.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        repository.Setup(r => r.ExistsByIdentificationNumberAsync("P2", customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new UpdateCustomerCommandHandler(repository.Object, unitOfWork.Object);
        var command = new UpdateCustomerCommand(customer.Id, "Alice Smith", IdentificationTypes.Passport, "P2", ValidAddress, "222", "smith@x.com", new DateOnly(1990, 1, 1), "Swedish", "Returning guest");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Customer.ShouldNotBeNull();
        result.Customer.FullName.ShouldBe("Alice Smith");
        result.Customer.Notes.ShouldBe("Returning guest");
        repository.Verify(r => r.Update(customer), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CustomerDoesNotExist_ReturnsErrorAndDoesNotSave()
    {
        var repository = new Mock<ICustomerRepository>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((CustomerEntity?)null);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new UpdateCustomerCommandHandler(repository.Object, unitOfWork.Object);
        var command = new UpdateCustomerCommand(Guid.NewGuid(), "Alice", IdentificationTypes.Passport, "P1", ValidAddress, "111", "alice@x.com", new DateOnly(1990, 1, 1), "Swedish", null);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Customer.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
