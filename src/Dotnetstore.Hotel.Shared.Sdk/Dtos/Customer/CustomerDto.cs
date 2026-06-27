using Dotnetstore.Hotel.Shared.Sdk.Dtos;

namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;

public record CustomerDto(
    Guid Id,
    int CustomerNumber,
    string FullName,
    string IdentificationType,
    string IdentificationNumber,
    AddressDto Address,
    string PhoneNumber,
    string Email,
    DateOnly DateOfBirth,
    string Nationality,
    string? Notes);
