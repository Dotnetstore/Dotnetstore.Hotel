using Dotnetstore.Hotel.Shared.Sdk.Dtos;

namespace Dotnetstore.Hotel.Shared.Sdk.Client;

public interface IHotelClient
{
    Task<HotelDto?> GetHotelAsync(Guid id, CancellationToken cancellationToken = default);

    Task<HotelDto> UpdateHotelAsync(Guid id, UpdateHotelRequest request, CancellationToken cancellationToken = default);
}
