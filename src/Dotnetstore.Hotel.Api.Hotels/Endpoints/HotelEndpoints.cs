using Dotnetstore.Hotel.Api.Hotels.Features.GetHotel;
using Dotnetstore.Hotel.Api.Hotels.Features.UpdateHotel;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;

namespace Dotnetstore.Hotel.Api.Hotels.Endpoints;

public static class HotelEndpoints
{
    public static IEndpointRouteBuilder MapHotelEndpoints(this IEndpointRouteBuilder app)
    {
        // Add .RequireAuthorization() here once JWT/RBAC is introduced.
        var group = app.MapGroup("/api/hotels");

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var hotel = await dispatcher.QueryAsync(new GetHotelQuery(id), cancellationToken);
            return hotel is null ? Results.NotFound() : Results.Ok(hotel);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateHotelRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var command = new UpdateHotelCommand(id, request.Name, request.Address, request.ContactInfo, request.Amenities);
            var hotel = await dispatcher.SendAsync(command, cancellationToken);
            return hotel is null ? Results.NotFound() : Results.Ok(hotel);
        });

        return app;
    }
}
