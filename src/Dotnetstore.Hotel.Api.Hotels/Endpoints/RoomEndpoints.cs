using Dotnetstore.Hotel.Api.Hotels.Features.CreateRoom;
using Dotnetstore.Hotel.Api.Hotels.Features.DeleteRoom;
using Dotnetstore.Hotel.Api.Hotels.Features.GetRoom;
using Dotnetstore.Hotel.Api.Hotels.Features.ListRooms;
using Dotnetstore.Hotel.Api.Hotels.Features.SearchRooms;
using Dotnetstore.Hotel.Api.Hotels.Features.UpdateRoom;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Endpoints;

public static class RoomEndpoints
{
    public static IEndpointRouteBuilder MapRoomEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/rooms");

        group.MapGet("/", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var rooms = await dispatcher.QueryAsync(new ListRoomsQuery(), cancellationToken);
            return Results.Ok(rooms);
        }).RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var room = await dispatcher.QueryAsync(new GetRoomQuery(id), cancellationToken);
            return room is null ? Results.NotFound() : Results.Ok(room);
        }).RequireAuthorization();

        group.MapGet("/search", async (DateOnly? checkInDate, DateOnly? checkOutDate, Guid[]? equipmentIds, Guid[]? tagIds, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var query = new SearchRoomsQuery(checkInDate, checkOutDate, equipmentIds ?? [], tagIds ?? []);
            var rooms = await dispatcher.QueryAsync(query, cancellationToken);
            return Results.Ok(rooms);
        }).RequireAuthorization("FrontDeskOnly");

        group.MapPost("/", async (CreateRoomRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var command = new CreateRoomCommand(request.RoomNumber, request.Floor, request.Capacity, request.BedType, request.PricePerNight, request.Status, request.Equipment);
            var result = await dispatcher.SendAsync(command, cancellationToken);
            return result.Room is null ? Results.BadRequest(result) : Results.Created($"/api/rooms/{result.Room.Id}", result);
        }).RequireAuthorization("AdminOnly");

        group.MapPut("/{id:guid}", async (Guid id, UpdateRoomRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var command = new UpdateRoomCommand(id, request.RoomNumber, request.Floor, request.Capacity, request.BedType, request.PricePerNight, request.Status, request.Equipment);
            var result = await dispatcher.SendAsync(command, cancellationToken);
            return result.Room is null ? Results.BadRequest(result) : Results.Ok(result);
        }).RequireAuthorization("AdminOnly");

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new DeleteRoomCommand(id), cancellationToken);
            return result.Succeeded ? Results.NoContent() : Results.BadRequest(result);
        }).RequireAuthorization("AdminOnly");

        return app;
    }
}
