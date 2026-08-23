using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Data;

namespace OrderManagement.Api.Endpoints;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers");

        group.MapGet("/", async (AppDbContext db) =>
            await db.Customers.OrderBy(c => c.Name).ToListAsync());

        return app;
    }
}
