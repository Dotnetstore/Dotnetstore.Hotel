using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.GetCustomer;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;
using CustomerEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class GetCustomerQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_CustomerExists_ReturnsMappedDto()
    {
        var address = new Address("Street 1", "City", "12345", "Country");
        var customer = CustomerEntity.Create(Guid.NewGuid(), "Alice", IdentificationTypes.Passport, "P1", address, "111", "alice@x.com", new DateOnly(1990, 1, 1), "Swedish", null);

        var repository = new Mock<ICustomerRepository>();
        repository.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var handler = new GetCustomerQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new GetCustomerQuery(customer.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result.FullName.ShouldBe("Alice");
        result.IdentificationNumber.ShouldBe("P1");
    }

    [Fact]
    public async Task HandleAsync_CustomerDoesNotExist_ReturnsNull()
    {
        var repository = new Mock<ICustomerRepository>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((CustomerEntity?)null);

        var handler = new GetCustomerQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new GetCustomerQuery(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeNull();
    }
}
