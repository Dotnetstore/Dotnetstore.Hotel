using Microsoft.EntityFrameworkCore;
using HotelEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Hotel;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

internal sealed class HotelRepository(HotelDbContext dbContext) : IHotelRepository
{
    public Task<HotelEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Hotels.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

    public void Update(HotelEntity hotel) => dbContext.Hotels.Update(hotel);
}
