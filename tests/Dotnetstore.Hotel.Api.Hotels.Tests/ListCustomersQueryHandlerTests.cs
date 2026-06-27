using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.ListCustomers;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;
using CustomerEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class ListCustomersQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsMappedDtoPerCustomer()
    {
        var address = new Address("Street 1", "City", "12345", "Country");
        var alice = CustomerEntity.Create(Guid.NewGuid(), "Alice", IdentificationTypes.Passport, "P1", address, "111", "alice@x.com", new DateOnly(1990, 1, 1), "Swedish", null);
        var bob = CustomerEntity.Create(Guid.NewGuid(), "Bob", IdentificationTypes.NationalId, "N1", address, "222", "bob@x.com", new DateOnly(1985, 5, 5), "Danish", "VIP");

        var repository = new Mock<ICustomerRepository>();
        repository.Setup(r => r.SearchAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync([alice, bob]);

        var handler = new ListCustomersQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new ListCustomersQuery(null), CancellationToken.None);

        result.Count.ShouldBe(2);
        result.Single(c => c.Id == alice.Id).FullName.ShouldBe("Alice");
        result.Single(c => c.Id == bob.Id).Notes.ShouldBe("VIP");
    }
}
