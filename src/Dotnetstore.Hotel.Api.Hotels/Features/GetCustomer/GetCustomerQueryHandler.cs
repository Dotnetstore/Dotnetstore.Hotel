using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Features.GetCustomer;

public class GetCustomerQueryHandler(ICustomerRepository customerRepository) : IQueryHandler<GetCustomerQuery, CustomerDto?>
{
    public async Task<CustomerDto?> HandleAsync(GetCustomerQuery query, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(query.Id, cancellationToken);
        return customer is null ? null : CustomerDtoMapper.ToDto(customer);
    }
}
