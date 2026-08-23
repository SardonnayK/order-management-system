using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Data;
using OrderManagement.Api.Models;
using OrderManagement.Api.Services;

namespace OrderManagement.Api.Endpoints;

public sealed record UpdateOrderStatusRequest(OrderStatus Status);

public sealed record UpdateOrderRequest(string? Notes, List<LineItem> Items);

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders");

        group.MapGet("/", async (OrderService orders) => await orders.GetOrdersAsync());

        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
            await db.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id) is { } order
                ? Results.Ok(order)
                : Results.NotFound());

        group.MapPost("/", async (Order order, OrderService orders) =>
        {
            var result = await orders.PlaceOrderAsync(order);
            return result switch
            {
                { Success: true, Value: { WasCreated: true } outcome } =>
                    Results.Created($"/api/orders/{outcome.Order.Id}", outcome.Order),
                // Idempotent resubmission: same customer + client reference returns
                // the already-stored order instead of creating a duplicate.
                { Success: true, Value: { } outcome } => Results.Ok(outcome.Order),
                _ => ToErrorResult(result.Error!, result.Kind),
            };
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateOrderRequest request, OrderService orders) =>
        {
            var result = await orders.UpdateOrderAsync(id, request.Notes, request.Items);
            return result.Success ? Results.Ok(result.Value) : ToErrorResult(result.Error!, result.Kind);
        });

        group.MapPatch("/{id:guid}/status", async (Guid id, UpdateOrderStatusRequest request, OrderService orders) =>
        {
            var result = await orders.UpdateStatusAsync(id, request.Status);
            return result.Success ? Results.Ok(result.Value) : ToErrorResult(result.Error!, result.Kind);
        });

        return app;
    }

    private static IResult ToErrorResult(string error, OrderErrorKind kind) => kind switch
    {
        OrderErrorKind.NotFound => Results.NotFound(new { message = error }),
        OrderErrorKind.Conflict => Results.Conflict(new { message = error }),
        _ => Results.BadRequest(new { message = error }),
    };
}
