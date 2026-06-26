using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;
using HotelEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Hotel;

namespace Dotnetstore.Hotel.Api.Hotels.Features.GetHotel;

public class GetHotelQueryHandler(IHotelRepository hotelRepository) : IQueryHandler<GetHotelQuery, HotelDto?>
{
    public async Task<HotelDto?> HandleAsync(GetHotelQuery query, CancellationToken cancellationToken)
    {
        var hotel = await hotelRepository.GetByIdAsync(query.HotelId, cancellationToken);
        return hotel is null ? null : ToDto(hotel);
    }

    internal static HotelDto ToDto(HotelEntity hotel) => new(
        hotel.Id,
        hotel.Name,
        new AddressDto(hotel.Address.Street, hotel.Address.City, hotel.Address.PostalCode, hotel.Address.Country),
        new ContactInfoDto(hotel.ContactInfo.PhoneNumber, hotel.ContactInfo.Email, hotel.ContactInfo.Website),
        hotel.Amenities);
}
