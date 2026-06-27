using Dotnetstore.Hotel.Shared.Sdk.Dtos;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

namespace Dotnetstore.Hotel.Shared.Sdk.Client;

public interface IHotelClient
{
    Task<HotelDto?> GetHotelAsync(Guid id, CancellationToken cancellationToken = default);

    Task<HotelDto> UpdateHotelAsync(Guid id, UpdateHotelRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EquipmentDto>> ListEquipmentAsync(string bearerToken, CancellationToken cancellationToken = default);

    Task<EquipmentDto?> GetEquipmentAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<CreateEquipmentResponse> CreateEquipmentAsync(CreateEquipmentRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<UpdateEquipmentResponse> UpdateEquipmentAsync(Guid id, UpdateEquipmentRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<bool> DeleteEquipmentAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);
}
