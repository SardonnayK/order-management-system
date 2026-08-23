using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Data;
using OrderManagement.Api.Models;

namespace OrderManagement.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders");

        group.MapGet("/", async (AppDbContext db) =>
            await db.Orders.OrderByDescending(o => o.CreatedAt).ToListAsync());

        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
            await db.Orders.FindAsync(id) is { } order ? Results.Ok(order) : Results.NotFound());

        group.MapPost("/", async (Order order, AppDbContext db) =>
        {
            db.Orders.Add(order);
            await db.SaveChangesAsync();
            return Results.Created($"/api/orders/{order.Id}", order);
        });

        return app;
    }
}
