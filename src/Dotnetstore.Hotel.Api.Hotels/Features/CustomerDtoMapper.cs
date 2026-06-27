using Dotnetstore.Hotel.Shared.Sdk.Dtos;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;
using CustomerEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Features;

internal static class CustomerDtoMapper
{
    public static CustomerDto ToDto(CustomerEntity customer) => new(
        customer.Id,
        customer.CustomerNumber,
        customer.FullName,
        customer.IdentificationType,
        customer.IdentificationNumber,
        new AddressDto(customer.Address.Street, customer.Address.City, customer.Address.PostalCode, customer.Address.Country),
        customer.PhoneNumber,
        customer.Email,
        customer.DateOfBirth,
        customer.Nationality,
        customer.Notes);
}
