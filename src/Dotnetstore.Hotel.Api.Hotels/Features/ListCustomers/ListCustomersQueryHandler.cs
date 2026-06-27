using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Features.ListCustomers;

public class ListCustomersQueryHandler(ICustomerRepository customerRepository) : IQueryHandler<ListCustomersQuery, IReadOnlyList<CustomerDto>>
{
    public async Task<IReadOnlyList<CustomerDto>> HandleAsync(ListCustomersQuery query, CancellationToken cancellationToken)
    {
        var customers = await customerRepository.SearchAsync(query.SearchTerm, cancellationToken);
        return customers.Select(CustomerDtoMapper.ToDto).ToList();
    }
}
