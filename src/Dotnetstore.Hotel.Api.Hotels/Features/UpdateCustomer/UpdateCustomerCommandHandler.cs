using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Features.UpdateCustomer;

public class UpdateCustomerCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCustomerCommand, UpdateCustomerResponse>
{
    public async Task<UpdateCustomerResponse> HandleAsync(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(command.Id, cancellationToken);
        if (customer is null)
        {
            return new UpdateCustomerResponse(null, ["Customer not found."]);
        }

        if (string.IsNullOrWhiteSpace(command.FullName))
        {
            return new UpdateCustomerResponse(null, ["Full name is required."]);
        }

        if (!IdentificationTypes.IsValid(command.IdentificationType))
        {
            return new UpdateCustomerResponse(null, [$"Identification type must be one of: {string.Join(", ", IdentificationTypes.All)}."]);
        }

        if (string.IsNullOrWhiteSpace(command.IdentificationNumber))
        {
            return new UpdateCustomerResponse(null, ["Identification number is required."]);
        }

        if (string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            return new UpdateCustomerResponse(null, ["Phone number is required."]);
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return new UpdateCustomerResponse(null, ["Email is required."]);
        }

        if (string.IsNullOrWhiteSpace(command.Nationality))
        {
            return new UpdateCustomerResponse(null, ["Nationality is required."]);
        }

        if (await customerRepository.ExistsByIdentificationNumberAsync(command.IdentificationNumber, command.Id, cancellationToken))
        {
            return new UpdateCustomerResponse(null, [$"Identification number '{command.IdentificationNumber}' is already in use."]);
        }

        var address = new Address(command.Address.Street, command.Address.City, command.Address.PostalCode, command.Address.Country);
        customer.UpdateDetails(
            command.FullName,
            command.IdentificationType,
            command.IdentificationNumber,
            address,
            command.PhoneNumber,
            command.Email,
            command.DateOfBirth,
            command.Nationality,
            command.Notes);

        customerRepository.Update(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateCustomerResponse(CustomerDtoMapper.ToDto(customer), []);
    }
}
