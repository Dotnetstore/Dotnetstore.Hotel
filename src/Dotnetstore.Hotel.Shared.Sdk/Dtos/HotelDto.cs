namespace Dotnetstore.Hotel.Shared.Sdk.Dtos;

public record HotelDto(
    Guid Id,
    string Name,
    AddressDto Address,
    ContactInfoDto ContactInfo,
    IReadOnlyCollection<string> Amenities);
