using OrderManagement.Api.Models;

namespace OrderManagement.Api.Services;

public static class OrderStatusRules
{
    // Fulfilled and Cancelled are terminal states.
    private static readonly Dictionary<OrderStatus, OrderStatus[]> Allowed = new()
    {
        [OrderStatus.Pending] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [OrderStatus.Fulfilled, OrderStatus.Cancelled],
        [OrderStatus.Fulfilled] = [],
        [OrderStatus.Cancelled] = [],
    };

    public static bool CanTransition(OrderStatus from, OrderStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
}
