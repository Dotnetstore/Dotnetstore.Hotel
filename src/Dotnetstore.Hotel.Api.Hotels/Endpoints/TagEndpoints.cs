using Dotnetstore.Hotel.Api.Hotels.Features.CreateTag;
using Dotnetstore.Hotel.Api.Hotels.Features.DeleteTag;
using Dotnetstore.Hotel.Api.Hotels.Features.GetTag;
using Dotnetstore.Hotel.Api.Hotels.Features.ListTags;
using Dotnetstore.Hotel.Api.Hotels.Features.UpdateTag;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Endpoints;

public static class TagEndpoints
{
    public static IEndpointRouteBuilder MapTagEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tags").RequireAuthorization("AdminOnly");

        group.MapGet("/", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var tags = await dispatcher.QueryAsync(new ListTagsQuery(), cancellationToken);
            return Results.Ok(tags);
        });

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var tag = await dispatcher.QueryAsync(new GetTagQuery(id), cancellationToken);
            return tag is null ? Results.NotFound() : Results.Ok(tag);
        });

        group.MapPost("/", async (CreateTagRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new CreateTagCommand(request.Name), cancellationToken);
            return result.Tag is null ? Results.BadRequest(result) : Results.Created($"/api/tags/{result.Tag.Id}", result);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateTagRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new UpdateTagCommand(id, request.Name), cancellationToken);
            return result.Tag is null ? Results.BadRequest(result) : Results.Ok(result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new DeleteTagCommand(id), cancellationToken);
            return result.Succeeded ? Results.NoContent() : Results.BadRequest(result);
        });

        return app;
    }
}
