using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Features.ListCustomers;

public record ListCustomersQuery(string? SearchTerm) : IQuery<IReadOnlyList<CustomerDto>>;
