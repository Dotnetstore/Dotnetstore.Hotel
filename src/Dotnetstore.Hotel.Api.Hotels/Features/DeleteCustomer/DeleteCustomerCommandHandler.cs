using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Features.DeleteCustomer;

public class DeleteCustomerCommandHandler(ICustomerRepository customerRepository, IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCustomerCommand, DeleteCustomerResponse>
{
    public async Task<DeleteCustomerResponse> HandleAsync(DeleteCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(command.Id, cancellationToken);
        if (customer is null)
        {
            return new DeleteCustomerResponse(false, ["Customer not found."]);
        }

        var activeBookings = await bookingRepository.CountActiveBookingsForCustomerAsync(command.Id, cancellationToken);
        if (activeBookings > 0)
        {
            return new DeleteCustomerResponse(false, [$"{activeBookings} active booking(s) still belong to this customer. Cancel them first."]);
        }

        customerRepository.Remove(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new DeleteCustomerResponse(true, []);
    }
}
