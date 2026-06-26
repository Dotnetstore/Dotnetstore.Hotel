using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.GetHotel;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;

namespace Dotnetstore.Hotel.Api.Hotels.Features.UpdateHotel;

public class UpdateHotelCommandHandler(IHotelRepository hotelRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateHotelCommand, HotelDto?>
{
    public async Task<HotelDto?> HandleAsync(UpdateHotelCommand command, CancellationToken cancellationToken)
    {
        var hotel = await hotelRepository.GetByIdAsync(command.HotelId, cancellationToken);
        if (hotel is null)
        {
            return null;
        }

        hotel.UpdateProfile(
            command.Name,
            new Address(command.Address.Street, command.Address.City, command.Address.PostalCode, command.Address.Country),
            new ContactInfo(command.ContactInfo.PhoneNumber, command.ContactInfo.Email, command.ContactInfo.Website));
        hotel.SetAmenities(command.Amenities);

        hotelRepository.Update(hotel);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return GetHotelQueryHandler.ToDto(hotel);
    }
}
