using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;

namespace Dotnetstore.Hotel.Api.Hotels.Features.GetHotel;

public record GetHotelQuery(Guid HotelId) : IQuery<HotelDto?>;
