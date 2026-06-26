using Dotnetstore.Hotel.Api.Users.Features.CreateUser;
using Dotnetstore.Hotel.Api.Users.Features.GetUser;
using Dotnetstore.Hotel.Api.Users.Features.RevokeUserTokens;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

namespace Dotnetstore.Hotel.Api.Users.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").RequireAuthorization("AdminOnly");

        group.MapPost("/", async (CreateUserRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var command = new CreateUserCommand(request.Email, request.UserName, request.Password, request.Role);
            var result = await dispatcher.SendAsync(command, cancellationToken);
            return result.User is null ? Results.BadRequest(result) : Results.Created($"/api/users/{result.User.Id}", result.User);
        });

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var user = await dispatcher.QueryAsync(new GetUserQuery(id), cancellationToken);
            return user is null ? Results.NotFound() : Results.Ok(user);
        });

        group.MapPost("/{id:guid}/revoke-tokens", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var revokedCount = await dispatcher.SendAsync(new RevokeUserTokensCommand(id), cancellationToken);
            return Results.Ok(new RevokeTokensResponse(revokedCount));
        });

        return app;
    }
}
