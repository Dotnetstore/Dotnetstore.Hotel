using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;
using CustomerEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CreateCustomer;

public class CreateCustomerCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCustomerCommand, CreateCustomerResponse>
{
    public async Task<CreateCustomerResponse> HandleAsync(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.FullName))
        {
            return new CreateCustomerResponse(null, ["Full name is required."]);
        }

        if (!IdentificationTypes.IsValid(command.IdentificationType))
        {
            return new CreateCustomerResponse(null, [$"Identification type must be one of: {string.Join(", ", IdentificationTypes.All)}."]);
        }

        if (string.IsNullOrWhiteSpace(command.IdentificationNumber))
        {
            return new CreateCustomerResponse(null, ["Identification number is required."]);
        }

        if (string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            return new CreateCustomerResponse(null, ["Phone number is required."]);
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return new CreateCustomerResponse(null, ["Email is required."]);
        }

        if (string.IsNullOrWhiteSpace(command.Nationality))
        {
            return new CreateCustomerResponse(null, ["Nationality is required."]);
        }

        if (await customerRepository.ExistsByIdentificationNumberAsync(command.IdentificationNumber, excludingId: null, cancellationToken))
        {
            return new CreateCustomerResponse(null, [$"Identification number '{command.IdentificationNumber}' is already in use."]);
        }

        var address = new Address(command.Address.Street, command.Address.City, command.Address.PostalCode, command.Address.Country);
        var customer = CustomerEntity.Create(
            Guid.NewGuid(),
            command.FullName,
            command.IdentificationType,
            command.IdentificationNumber,
            address,
            command.PhoneNumber,
            command.Email,
            command.DateOfBirth,
            command.Nationality,
            command.Notes);

        await customerRepository.AddAsync(customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateCustomerResponse(CustomerDtoMapper.ToDto(customer), []);
    }
}
