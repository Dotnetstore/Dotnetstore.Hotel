using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.DeleteCustomer;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;
using CustomerEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class DeleteCustomerCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_CustomerExistsWithNoActiveBookings_RemovesAndSaves()
    {
        var customer = CustomerEntity.Create(Guid.NewGuid(), "Alice", IdentificationTypes.Passport, "P1", new Address("S", "C", "Z", "Co"), "111", "alice@x.com", new DateOnly(1990, 1, 1), "Swedish", null);

        var customerRepository = new Mock<ICustomerRepository>();
        customerRepository.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository.Setup(r => r.CountActiveBookingsForCustomerAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeleteCustomerCommandHandler(customerRepository.Object, bookingRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new DeleteCustomerCommand(customer.Id), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        customerRepository.Verify(r => r.Remove(customer), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CustomerHasActiveBookings_ReturnsErrorAndDoesNotSave()
    {
        var customer = CustomerEntity.Create(Guid.NewGuid(), "Alice", IdentificationTypes.Passport, "P1", new Address("S", "C", "Z", "Co"), "111", "alice@x.com", new DateOnly(1990, 1, 1), "Swedish", null);

        var customerRepository = new Mock<ICustomerRepository>();
        customerRepository.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository.Setup(r => r.CountActiveBookingsForCustomerAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(2);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeleteCustomerCommandHandler(customerRepository.Object, bookingRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new DeleteCustomerCommand(customer.Id), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        customerRepository.Verify(r => r.Remove(It.IsAny<CustomerEntity>()), Times.Never);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_CustomerDoesNotExist_ReturnsErrorAndDoesNotSave()
    {
        var customerRepository = new Mock<ICustomerRepository>();
        customerRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((CustomerEntity?)null);

        var bookingRepository = new Mock<IBookingRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeleteCustomerCommandHandler(customerRepository.Object, bookingRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new DeleteCustomerCommand(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
