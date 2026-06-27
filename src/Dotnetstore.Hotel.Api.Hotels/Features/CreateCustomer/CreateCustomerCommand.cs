using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CreateCustomer;

public record CreateCustomerCommand(
    string FullName,
    string IdentificationType,
    string IdentificationNumber,
    AddressDto Address,
    string PhoneNumber,
    string Email,
    DateOnly DateOfBirth,
    string Nationality,
    string? Notes) : ICommand<CreateCustomerResponse>;
