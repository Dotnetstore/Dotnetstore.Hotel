namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;

public record CreateCustomerResponse(CustomerDto? Customer, IReadOnlyList<string> Errors);
