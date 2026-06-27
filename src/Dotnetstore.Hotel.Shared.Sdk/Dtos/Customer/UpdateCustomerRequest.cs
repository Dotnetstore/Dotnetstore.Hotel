using Dotnetstore.Hotel.Shared.Sdk.Dtos;

namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;

public record UpdateCustomerRequest(
    string FullName,
    string IdentificationType,
    string IdentificationNumber,
    AddressDto Address,
    string PhoneNumber,
    string Email,
    DateOnly DateOfBirth,
    string Nationality,
    string? Notes);
