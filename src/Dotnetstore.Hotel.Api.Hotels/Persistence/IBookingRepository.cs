using Dotnetstore.Hotel.Api.Hotels.Domain;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<Booking>> GetAllAsync(Guid? customerId, CancellationToken cancellationToken);

    Task AddAsync(Booking booking, CancellationToken cancellationToken);

    void Update(Booking booking);

    Task<bool> HasOverlappingBookingAsync(Guid roomId, DateOnly checkInDate, DateOnly checkOutDate, Guid? excludingBookingId, CancellationToken cancellationToken);

    Task<int> CountActiveBookingsForCustomerAsync(Guid customerId, CancellationToken cancellationToken);

    Task<int> CountActiveBookingsForRoomAsync(Guid roomId, CancellationToken cancellationToken);
}
