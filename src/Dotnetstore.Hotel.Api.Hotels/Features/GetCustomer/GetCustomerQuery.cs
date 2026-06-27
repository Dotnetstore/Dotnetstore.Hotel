using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Features.GetCustomer;

public record GetCustomerQuery(Guid Id) : IQuery<CustomerDto?>;
