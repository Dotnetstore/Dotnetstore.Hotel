using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;

namespace Dotnetstore.Hotel.Api.Hotels.Features.UpdateHotel;

public record UpdateHotelCommand(
    Guid HotelId,
    string Name,
    AddressDto Address,
    ContactInfoDto ContactInfo,
    IReadOnlyCollection<string> Amenities) : ICommand<HotelDto?>;
