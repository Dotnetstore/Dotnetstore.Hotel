using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.CreateCustomer;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;
using Moq;
using Shouldly;
using CustomerEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class CreateCustomerCommandHandlerTests
{
    private static readonly AddressDto ValidAddress = new("Street 1", "City", "12345", "Country");

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesAndSaves()
    {
        var repository = new Mock<ICustomerRepository>();
        repository.Setup(r => r.ExistsByIdentificationNumberAsync("P1", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateCustomerCommandHandler(repository.Object, unitOfWork.Object);
        var command = new CreateCustomerCommand("Alice", IdentificationTypes.Passport, "P1", ValidAddress, "111", "alice@x.com", new DateOnly(1990, 1, 1), "Swedish", null);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Customer.ShouldNotBeNull();
        result.Customer.FullName.ShouldBe("Alice");
        result.Errors.ShouldBeEmpty();
        repository.Verify(r => r.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_BlankFullName_ReturnsErrorAndDoesNotSave()
    {
        var repository = new Mock<ICustomerRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateCustomerCommandHandler(repository.Object, unitOfWork.Object);
        var command = new CreateCustomerCommand("   ", IdentificationTypes.Passport, "P1", ValidAddress, "111", "alice@x.com", new DateOnly(1990, 1, 1), "Swedish", null);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Customer.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_InvalidIdentificationType_ReturnsErrorAndDoesNotSave()
    {
        var repository = new Mock<ICustomerRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateCustomerCommandHandler(repository.Object, unitOfWork.Object);
        var command = new CreateCustomerCommand("Alice", "DriversLicense", "P1", ValidAddress, "111", "alice@x.com", new DateOnly(1990, 1, 1), "Swedish", null);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Customer.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_DuplicateIdentificationNumber_ReturnsErrorAndDoesNotSave()
    {
        var repository = new Mock<ICustomerRepository>();
        repository.Setup(r => r.ExistsByIdentificationNumberAsync("P1", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateCustomerCommandHandler(repository.Object, unitOfWork.Object);
        var command = new CreateCustomerCommand("Alice", IdentificationTypes.Passport, "P1", ValidAddress, "111", "alice@x.com", new DateOnly(1990, 1, 1), "Swedish", null);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Customer.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
