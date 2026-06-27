using System.Security.Claims;
using Dotnetstore.Hotel.Api.Users.Features.ActivateUser;
using Dotnetstore.Hotel.Api.Users.Features.CreateUser;
using Dotnetstore.Hotel.Api.Users.Features.DeactivateUser;
using Dotnetstore.Hotel.Api.Users.Features.GetUser;
using Dotnetstore.Hotel.Api.Users.Features.ListUsers;
using Dotnetstore.Hotel.Api.Users.Features.RevokeUserTokens;
using Dotnetstore.Hotel.Api.Users.Features.UpdateUser;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

namespace Dotnetstore.Hotel.Api.Users.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").RequireAuthorization("AdminOnly");

        group.MapGet("/", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var users = await dispatcher.QueryAsync(new ListUsersQuery(), cancellationToken);
            return Results.Ok(users);
        });

        group.MapPost("/", async (CreateUserRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var command = new CreateUserCommand(request.Email, request.UserName, request.Password, request.Role);
            var result = await dispatcher.SendAsync(command, cancellationToken);
            return result.User is null ? Results.BadRequest(result) : Results.Created($"/api/users/{result.User.Id}", result);
        });

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var user = await dispatcher.QueryAsync(new GetUserQuery(id), cancellationToken);
            return user is null ? Results.NotFound() : Results.Ok(user);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var command = new UpdateUserCommand(id, request.Email, request.UserName, request.Role, request.NewPassword);
            var result = await dispatcher.SendAsync(command, cancellationToken);
            return result.User is null ? Results.BadRequest(result) : Results.Ok(result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal currentUser, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var requestedByUserId = GetUserId(currentUser);
            var deactivated = await dispatcher.SendAsync(new DeactivateUserCommand(id, requestedByUserId), cancellationToken);
            return deactivated ? Results.NoContent() : Results.BadRequest("User not found or cannot deactivate your own account.");
        });

        group.MapPost("/{id:guid}/activate", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var activated = await dispatcher.SendAsync(new ActivateUserCommand(id), cancellationToken);
            return activated ? Results.NoContent() : Results.NotFound();
        });

        group.MapPost("/{id:guid}/revoke-tokens", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var revokedCount = await dispatcher.SendAsync(new RevokeUserTokensCommand(id), cancellationToken);
            return Results.Ok(new RevokeTokensResponse(revokedCount));
        });

        return app;
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }
}
