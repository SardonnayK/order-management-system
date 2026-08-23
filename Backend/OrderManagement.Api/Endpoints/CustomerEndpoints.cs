using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Data;
using OrderManagement.Api.Models;

namespace OrderManagement.Api.Endpoints;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers");

        group.MapGet("/", async (AppDbContext db) =>
            await db.Customers.OrderBy(c => c.Name).ToListAsync());

        group.MapPost("/", async (Customer customer, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(customer.Id) ||
                string.IsNullOrWhiteSpace(customer.Name) ||
                string.IsNullOrWhiteSpace(customer.Email))
            {
                return Results.BadRequest(new { message = "A customer requires an id, a name and an email." });
            }

            if (await db.Customers.FindAsync(customer.Id) is not null)
            {
                return Results.Conflict(new { message = $"A customer with id '{customer.Id}' already exists." });
            }

            db.Customers.Add(customer);
            await db.SaveChangesAsync();
            return Results.Created($"/api/customers/{customer.Id}", customer);
        });

        return app;
    }
}
