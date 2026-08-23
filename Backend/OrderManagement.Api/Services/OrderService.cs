using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Data;
using OrderManagement.Api.Models;

namespace OrderManagement.Api.Services;

public sealed record PlaceOrderOutcome(Order Order, bool WasCreated);

public class OrderService(AppDbContext db)
{
    /// <summary>
    /// Places an order idempotently: resubmitting the same customer + client
    /// reference returns the already-stored order instead of creating a duplicate.
    /// </summary>
    public async Task<OrderResult<PlaceOrderOutcome>> PlaceOrderAsync(Order order)
    {
        if (order.Customer is null || string.IsNullOrWhiteSpace(order.Customer.Id))
        {
            return OrderResult<PlaceOrderOutcome>.Fail(
                "An order cannot exist without a customer. Provide a customer with a non-empty id.",
                OrderErrorKind.Validation);
        }

        var customerId = order.Customer.Id;

        if (await FindByReferenceAsync(customerId, order.ClientReference) is { } existing)
        {
            return OrderResult<PlaceOrderOutcome>.Ok(new PlaceOrderOutcome(existing, WasCreated: false));
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
        if (await db.Customers.FindAsync(customerId) is { } existingCustomer)
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
            // Lost a race against a concurrent identical submission: the unique
            // (CustomerId, ClientReference) index rejected us. Stay idempotent and
            // return the order that won.
            db.ChangeTracker.Clear();
            if (await FindByReferenceAsync(customerId, order.ClientReference) is { } winner)
            {
                return OrderResult<PlaceOrderOutcome>.Ok(new PlaceOrderOutcome(winner, WasCreated: false));
            }

            return OrderResult<PlaceOrderOutcome>.Fail(
                "The order could not be saved because it violates a database constraint.",
                OrderErrorKind.Conflict);
        }

        return OrderResult<PlaceOrderOutcome>.Ok(new PlaceOrderOutcome(order, WasCreated: true));
    }

    public async Task<OrderResult<Order>> UpdateStatusAsync(Guid id, OrderStatus target)
    {
        var order = await db.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return OrderResult<Order>.Fail($"Order '{id}' was not found.", OrderErrorKind.NotFound);
        }

        if (!OrderStatusRules.CanTransition(order.Status, target))
        {
            return OrderResult<Order>.Fail(
                $"Cannot change order status from {order.Status} to {target}.",
                OrderErrorKind.Conflict);
        }

        order.Status = target;
        await db.SaveChangesAsync();
        return OrderResult<Order>.Ok(order);
    }

    private Task<Order?> FindByReferenceAsync(string customerId, string clientReference) =>
        db.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o =>
                o.ClientReference == clientReference &&
                EF.Property<string>(o, "CustomerId") == customerId);
}
