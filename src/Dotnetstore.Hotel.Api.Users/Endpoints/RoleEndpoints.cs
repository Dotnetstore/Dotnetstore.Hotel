using Dotnetstore.Hotel.Api.Users.Features.CreateRole;
using Dotnetstore.Hotel.Api.Users.Features.DeleteRole;
using Dotnetstore.Hotel.Api.Users.Features.GetRole;
using Dotnetstore.Hotel.Api.Users.Features.ListRoles;
using Dotnetstore.Hotel.Api.Users.Features.UpdateRole;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;

namespace Dotnetstore.Hotel.Api.Users.Endpoints;

public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles").RequireAuthorization("AdminOnly");

        group.MapGet("/", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var roles = await dispatcher.QueryAsync(new ListRolesQuery(), cancellationToken);
            return Results.Ok(roles);
        });

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var role = await dispatcher.QueryAsync(new GetRoleQuery(id), cancellationToken);
            return role is null ? Results.NotFound() : Results.Ok(role);
        });

        group.MapPost("/", async (CreateRoleRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new CreateRoleCommand(request.Name), cancellationToken);
            return result.Role is null ? Results.BadRequest(result) : Results.Created($"/api/roles/{result.Role.Id}", result);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateRoleRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new UpdateRoleCommand(id, request.Name), cancellationToken);
            return result.Role is null ? Results.BadRequest(result) : Results.Ok(result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new DeleteRoleCommand(id), cancellationToken);
            return result.Succeeded ? Results.NoContent() : Results.BadRequest(result);
        });

        return app;
    }
}
