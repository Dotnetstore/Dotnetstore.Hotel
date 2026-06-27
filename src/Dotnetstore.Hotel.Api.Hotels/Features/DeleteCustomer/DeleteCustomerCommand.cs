using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Features.DeleteCustomer;

public record DeleteCustomerCommand(Guid Id) : ICommand<DeleteCustomerResponse>;
