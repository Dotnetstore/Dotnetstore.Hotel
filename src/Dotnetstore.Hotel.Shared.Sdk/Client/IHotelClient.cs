using Dotnetstore.Hotel.Shared.Sdk.Dtos;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;

namespace Dotnetstore.Hotel.Shared.Sdk.Client;

public interface IHotelClient
{
    Task<HotelDto?> GetHotelAsync(Guid id, CancellationToken cancellationToken = default);

    Task<HotelDto> UpdateHotelAsync(Guid id, UpdateHotelRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EquipmentDto>> ListEquipmentAsync(string bearerToken, CancellationToken cancellationToken = default);

    Task<EquipmentDto?> GetEquipmentAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<CreateEquipmentResponse> CreateEquipmentAsync(CreateEquipmentRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<UpdateEquipmentResponse> UpdateEquipmentAsync(Guid id, UpdateEquipmentRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<DeleteEquipmentResponse> DeleteEquipmentAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoomDto>> ListRoomsAsync(string bearerToken, CancellationToken cancellationToken = default);

    Task<RoomDto?> GetRoomAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<CreateRoomResponse> CreateRoomAsync(CreateRoomRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<UpdateRoomResponse> UpdateRoomAsync(Guid id, UpdateRoomRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<DeleteRoomResponse> DeleteRoomAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoomDto>> SearchRoomsAsync(
        DateOnly? checkInDate,
        DateOnly? checkOutDate,
        IReadOnlyCollection<Guid> equipmentIds,
        IReadOnlyCollection<Guid> tagIds,
        string bearerToken,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TagDto>> ListTagsAsync(string bearerToken, CancellationToken cancellationToken = default);

    Task<TagDto?> GetTagAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<CreateTagResponse> CreateTagAsync(CreateTagRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<UpdateTagResponse> UpdateTagAsync(Guid id, UpdateTagRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<DeleteTagResponse> DeleteTagAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerDto>> ListCustomersAsync(string? searchTerm, string bearerToken, CancellationToken cancellationToken = default);

    Task<CustomerDto?> GetCustomerAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<CreateCustomerResponse> CreateCustomerAsync(CreateCustomerRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<UpdateCustomerResponse> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<DeleteCustomerResponse> DeleteCustomerAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookingDto>> ListBookingsAsync(Guid? customerId, string bearerToken, CancellationToken cancellationToken = default);

    Task<BookingDto?> GetBookingAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<CreateBookingResponse> CreateBookingAsync(CreateBookingRequest request, string bearerToken, CancellationToken cancellationToken = default);

    Task<BookingActionResponse> CancelBookingAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<BookingActionResponse> CheckInBookingAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);

    Task<BookingActionResponse> CheckOutBookingAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default);
}
