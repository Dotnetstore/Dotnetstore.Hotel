using Dotnetstore.Hotel.Api.Hotels.Features.CancelBooking;
using Dotnetstore.Hotel.Api.Hotels.Features.CheckInBooking;
using Dotnetstore.Hotel.Api.Hotels.Features.CheckOutBooking;
using Dotnetstore.Hotel.Api.Hotels.Features.CreateBooking;
using Dotnetstore.Hotel.Api.Hotels.Features.GetBooking;
using Dotnetstore.Hotel.Api.Hotels.Features.ListBookings;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

namespace Dotnetstore.Hotel.Api.Hotels.Endpoints;

public static class BookingEndpoints
{
    public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings").RequireAuthorization("FrontDeskOnly");

        group.MapGet("/", async (Guid? customerId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var bookings = await dispatcher.QueryAsync(new ListBookingsQuery(customerId), cancellationToken);
            return Results.Ok(bookings);
        });

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var booking = await dispatcher.QueryAsync(new GetBookingQuery(id), cancellationToken);
            return booking is null ? Results.NotFound() : Results.Ok(booking);
        });

        group.MapPost("/", async (CreateBookingRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var command = new CreateBookingCommand(request.CustomerId, request.CheckInDate, request.CheckOutDate, request.RoomIds);
            var result = await dispatcher.SendAsync(command, cancellationToken);
            return result.Booking is null ? Results.BadRequest(result) : Results.Created($"/api/bookings/{result.Booking.Id}", result);
        });

        group.MapPost("/{id:guid}/cancel", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new CancelBookingCommand(id), cancellationToken);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        });

        group.MapPost("/{id:guid}/check-in", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new CheckInBookingCommand(id), cancellationToken);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        });

        group.MapPost("/{id:guid}/check-out", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new CheckOutBookingCommand(id), cancellationToken);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        });

        return app;
    }
}
