using Dotnetstore.Hotel.Api.Hotels.Features.CreateCustomer;
using Dotnetstore.Hotel.Api.Hotels.Features.DeleteCustomer;
using Dotnetstore.Hotel.Api.Hotels.Features.GetCustomer;
using Dotnetstore.Hotel.Api.Hotels.Features.ListCustomers;
using Dotnetstore.Hotel.Api.Hotels.Features.UpdateCustomer;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;

namespace Dotnetstore.Hotel.Api.Hotels.Endpoints;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers").RequireAuthorization("FrontDeskOnly");

        group.MapGet("/", async (string? search, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var customers = await dispatcher.QueryAsync(new ListCustomersQuery(search), cancellationToken);
            return Results.Ok(customers);
        });

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var customer = await dispatcher.QueryAsync(new GetCustomerQuery(id), cancellationToken);
            return customer is null ? Results.NotFound() : Results.Ok(customer);
        });

        group.MapPost("/", async (CreateCustomerRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var command = new CreateCustomerCommand(
                request.FullName, request.IdentificationType, request.IdentificationNumber, request.Address,
                request.PhoneNumber, request.Email, request.DateOfBirth, request.Nationality, request.Notes);
            var result = await dispatcher.SendAsync(command, cancellationToken);
            return result.Customer is null ? Results.BadRequest(result) : Results.Created($"/api/customers/{result.Customer.Id}", result);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateCustomerRequest request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var command = new UpdateCustomerCommand(
                id, request.FullName, request.IdentificationType, request.IdentificationNumber, request.Address,
                request.PhoneNumber, request.Email, request.DateOfBirth, request.Nationality, request.Notes);
            var result = await dispatcher.SendAsync(command, cancellationToken);
            return result.Customer is null ? Results.BadRequest(result) : Results.Ok(result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new DeleteCustomerCommand(id), cancellationToken);
            return result.Succeeded ? Results.NoContent() : Results.BadRequest(result);
        });

        return app;
    }
}
