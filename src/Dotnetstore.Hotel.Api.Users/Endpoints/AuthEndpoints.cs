using Dotnetstore.Hotel.Api.Users.Features.Login;
using Dotnetstore.Hotel.Api.Users.Features.Logout;
using Dotnetstore.Hotel.Api.Users.Features.Refresh;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;

namespace Dotnetstore.Hotel.Api.Users.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (LoginRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new LoginCommand(request.Email, request.Password), cancellationToken);
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        }).AllowAnonymous();

        app.MapPost("/api/auth/refresh", async (RefreshTokenRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new RefreshTokenCommand(request.RefreshToken), cancellationToken);
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        }).AllowAnonymous();

        app.MapPost("/api/auth/logout", async (RefreshTokenRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            await dispatcher.SendAsync(new LogoutCommand(request.RefreshToken), cancellationToken);
            return Results.NoContent();
        }).AllowAnonymous();

        return app;
    }
}
