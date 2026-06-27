namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;

public record DeleteCustomerResponse(bool Succeeded, IReadOnlyList<string> Errors);
