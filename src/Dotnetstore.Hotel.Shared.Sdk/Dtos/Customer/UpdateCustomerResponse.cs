namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;

public record UpdateCustomerResponse(CustomerDto? Customer, IReadOnlyList<string> Errors);
