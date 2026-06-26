namespace Dotnetstore.Hotel.Shared.Sdk.Dtos;

public record UpdateHotelRequest(
    string Name,
    AddressDto Address,
    ContactInfoDto ContactInfo,
    IReadOnlyCollection<string> Amenities);
