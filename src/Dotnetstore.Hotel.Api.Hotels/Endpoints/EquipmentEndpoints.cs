using Dotnetstore.Hotel.Api.Hotels.Features.CreateEquipment;
using Dotnetstore.Hotel.Api.Hotels.Features.DeleteEquipment;
using Dotnetstore.Hotel.Api.Hotels.Features.GetEquipment;
using Dotnetstore.Hotel.Api.Hotels.Features.ListEquipment;
using Dotnetstore.Hotel.Api.Hotels.Features.UpdateEquipment;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

namespace Dotnetstore.Hotel.Api.Hotels.Endpoints;

public static class EquipmentEndpoints
{
    public static IEndpointRouteBuilder MapEquipmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/equipment").RequireAuthorization("AdminOnly");

        group.MapGet("/", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var equipment = await dispatcher.QueryAsync(new ListEquipmentQuery(), cancellationToken);
            return Results.Ok(equipment);
        });

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var equipment = await dispatcher.QueryAsync(new GetEquipmentQuery(id), cancellationToken);
            return equipment is null ? Results.NotFound() : Results.Ok(equipment);
        });

        group.MapPost("/", async (CreateEquipmentRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new CreateEquipmentCommand(request.Name, request.Description), cancellationToken);
            return result.Equipment is null ? Results.BadRequest(result) : Results.Created($"/api/equipment/{result.Equipment.Id}", result);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateEquipmentRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new UpdateEquipmentCommand(id, request.Name, request.Description), cancellationToken);
            return result.Equipment is null ? Results.BadRequest(result) : Results.Ok(result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new DeleteEquipmentCommand(id), cancellationToken);
            return result.Succeeded ? Results.NoContent() : Results.BadRequest(result);
        });

        return app;
    }
}
