using HotelEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Hotel;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

public interface IHotelRepository
{
    Task<HotelEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Update(HotelEntity hotel);
}
