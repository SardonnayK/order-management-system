using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Data;
using OrderManagement.Api.Models;

namespace OrderManagement.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders");

        // Items is an owned collection, so EF includes it automatically.
        group.MapGet("/", async (AppDbContext db) =>
            await db.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.CreatedAtUtc)
                .ToListAsync());

        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
            await db.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id) is { } order
                ? Results.Ok(order)
                : Results.NotFound());

        group.MapPost("/", async (Order order, AppDbContext db) =>
        {
            if (order.Customer is null || string.IsNullOrWhiteSpace(order.Customer.Id))
            {
                return Results.BadRequest(new
                {
                    message = "An order cannot exist without a customer. Provide a customer with a non-empty id."
                });
            }

            if (order.Id == Guid.Empty)
            {
                order.Id = Guid.NewGuid();
            }

            if (order.CreatedAtUtc == default)
            {
                order.CreatedAtUtc = DateTime.UtcNow;
            }

            // Reuse the existing customer row instead of inserting a duplicate.
            if (await db.Customers.FindAsync(order.Customer.Id) is { } existingCustomer)
            {
                order.Customer = existingCustomer;
            }

            db.Orders.Add(order);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
            {
                return Results.Conflict(new
                {
                    message = $"An order with client reference '{order.ClientReference}' already exists for customer '{order.Customer.Id}'."
                });
            }

            return Results.Created($"/api/orders/{order.Id}", order);
        });

        return app;
    }
}
