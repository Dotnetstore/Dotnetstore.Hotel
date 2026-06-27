using Dotnetstore.Hotel.Api.Hotels.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

internal sealed class BookingRepository(HotelDbContext dbContext) : IBookingRepository
{
    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Rooms)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<List<Booking>> GetAllAsync(Guid? customerId, CancellationToken cancellationToken)
        => dbContext.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Rooms)
            .Where(b => customerId == null || b.CustomerId == customerId)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Booking booking, CancellationToken cancellationToken)
    {
        dbContext.Bookings.Add(booking);
        return Task.CompletedTask;
    }

    public void Update(Booking booking) => dbContext.Bookings.Update(booking);

    public Task<bool> HasOverlappingBookingAsync(Guid roomId, DateOnly checkInDate, DateOnly checkOutDate, Guid? excludingBookingId, CancellationToken cancellationToken)
        => dbContext.Bookings.AnyAsync(
            b => b.Id != excludingBookingId
                 && BookingStatuses.Active.Contains(b.Status)
                 && b.Rooms.Any(r => r.Id == roomId)
                 && b.CheckInDate < checkOutDate
                 && checkInDate < b.CheckOutDate,
            cancellationToken);

    public Task<int> CountActiveBookingsForCustomerAsync(Guid customerId, CancellationToken cancellationToken)
        => dbContext.Bookings.CountAsync(
            b => b.CustomerId == customerId && BookingStatuses.Active.Contains(b.Status),
            cancellationToken);

    public Task<int> CountActiveBookingsForRoomAsync(Guid roomId, CancellationToken cancellationToken)
        => dbContext.Bookings.CountAsync(
            b => b.Rooms.Any(r => r.Id == roomId) && BookingStatuses.Active.Contains(b.Status),
            cancellationToken);
}
