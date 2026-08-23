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

        if (ValidateItems(order.Items) is { } itemError)
        {
            return OrderResult<PlaceOrderOutcome>.Fail(itemError, OrderErrorKind.Validation);
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

    /// <summary>
    /// Updates the editable parts of an order (notes, line item quantities/prices).
    /// Rejected once the order is in a finalized state (Fulfilled or Cancelled).
    /// </summary>
    public async Task<OrderResult<Order>> UpdateOrderAsync(Guid id, string? notes, List<LineItem> items)
    {
        var order = await db.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return OrderResult<Order>.Fail($"Order '{id}' was not found.", OrderErrorKind.NotFound);
        }

        if (order.Status is OrderStatus.Fulfilled or OrderStatus.Cancelled)
        {
            return OrderResult<Order>.Fail(
                $"Order '{id}' is {order.Status} and can no longer be edited.",
                OrderErrorKind.Conflict);
        }

        if (ValidateItems(items) is { } itemError)
        {
            return OrderResult<Order>.Fail(itemError, OrderErrorKind.Validation);
        }

        order.Notes = notes;
        order.Items.Clear();
        order.Items.AddRange(items);
        await db.SaveChangesAsync();
        return OrderResult<Order>.Ok(order);
    }

    private static string? ValidateItems(List<LineItem> items)
    {
        if (items.Count == 0)
        {
            return "An order must have at least one line item.";
        }

        if (items.Any(i => i.Quantity < 1 || i.UnitPrice <= 0))
        {
            return "Line items must have a quantity of at least 1 and a positive unit price.";
        }

        return null;
    }

    private Task<Order?> FindByReferenceAsync(string customerId, string clientReference) =>
        db.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o =>
                o.ClientReference == clientReference &&
                EF.Property<string>(o, "CustomerId") == customerId);
}
