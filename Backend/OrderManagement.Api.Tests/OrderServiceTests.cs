using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Data;
using OrderManagement.Api.Models;
using OrderManagement.Api.Services;

namespace OrderManagement.Api.Tests;

public sealed class OrderServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly AppDbContext _db;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        // SQLite in-memory: same engine and constraints (unique indexes!) as production.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        _db = new AppDbContext(_options);
        _db.Database.EnsureCreated();
        _service = new OrderService(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private static Order NewOrder(string clientReference = "REF-100") => new()
    {
        ClientReference = clientReference,
        Customer = new Customer { Id = "cust-001", Email = "jane@example.com", Name = "Jane Doe" },
        Currency = "ZAR",
        Items =
        [
            new LineItem { Sku = "SKU-A", Name = "Widget", Quantity = 2, UnitPrice = 10.00m },
            new LineItem { Sku = "SKU-B", Name = "Gadget", Quantity = 1, UnitPrice = 5.00m },
        ],
    };

    [Fact]
    public async Task PlacingSameClientReferenceTwice_ReturnsSameOrderId_WithoutCreatingSecondRecord()
    {
        var first = await _service.PlaceOrderAsync(NewOrder("REF-100"));
        var second = await _service.PlaceOrderAsync(NewOrder("REF-100"));

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.True(first.Value!.WasCreated);
        Assert.False(second.Value!.WasCreated);
        Assert.Equal(first.Value.Order.Id, second.Value.Order.Id);
        Assert.Equal(1, await _db.Orders.CountAsync());
    }

    [Fact]
    public async Task PlacedOrder_ComputesSubtotalAndTotal_OnTheServer()
    {
        // 2 x R10.00 + 1 x R5.00 = R25.00
        var placed = await _service.PlaceOrderAsync(NewOrder());
        Assert.True(placed.Success);

        // Read back through a fresh context so the values come from persisted state,
        // not the instance the test built.
        using var freshDb = new AppDbContext(_options);
        var loaded = await freshDb.Orders.Include(o => o.Customer).SingleAsync();

        Assert.Equal(25.00m, loaded.Subtotal);
        Assert.Equal(25.00m, loaded.Total);
    }

    [Fact]
    public async Task GettingOrders_ReturnsThemNewestFirst()
    {
        // Placed deliberately out of chronological order.
        var middle = NewOrder("REF-MIDDLE");
        middle.CreatedAtUtc = new DateTime(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);
        var oldest = NewOrder("REF-OLDEST");
        oldest.CreatedAtUtc = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);
        var newest = NewOrder("REF-NEWEST");
        newest.CreatedAtUtc = new DateTime(2026, 8, 23, 12, 0, 0, DateTimeKind.Utc);

        foreach (var order in new[] { middle, oldest, newest })
        {
            var placed = await _service.PlaceOrderAsync(order);
            Assert.True(placed.Success);
        }

        var orders = await _service.GetOrdersAsync();

        Assert.Equal(
            new[] { "REF-NEWEST", "REF-MIDDLE", "REF-OLDEST" },
            orders.Select(o => o.ClientReference).ToArray());
    }

    [Theory]
    // From Pending
    [InlineData(OrderStatus.Pending, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Fulfilled, false)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
    // From Confirmed
    [InlineData(OrderStatus.Confirmed, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Confirmed, false)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Fulfilled, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled, true)]
    // From Fulfilled (terminal)
    [InlineData(OrderStatus.Fulfilled, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Fulfilled, OrderStatus.Confirmed, false)]
    [InlineData(OrderStatus.Fulfilled, OrderStatus.Fulfilled, false)]
    [InlineData(OrderStatus.Fulfilled, OrderStatus.Cancelled, false)]
    // From Cancelled (terminal) — includes the critical Cancelled -> Confirmed case
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Confirmed, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Fulfilled, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Cancelled, false)]
    public async Task UpdatingStatus_EnforcesEveryTransitionRule(
        OrderStatus from, OrderStatus to, bool allowed)
    {
        var order = NewOrder();
        order.Status = from;
        var placed = await _service.PlaceOrderAsync(order);
        var id = placed.Value!.Order.Id;

        var result = await _service.UpdateStatusAsync(id, to);

        using var freshDb = new AppDbContext(_options);
        var stored = await freshDb.Orders.SingleAsync(o => o.Id == id);

        if (allowed)
        {
            Assert.True(result.Success);
            Assert.Equal(to, result.Value!.Status);
            Assert.Equal(to, stored.Status);
        }
        else
        {
            Assert.False(result.Success);
            Assert.Equal(OrderErrorKind.Conflict, result.Kind);
            Assert.Contains(from.ToString(), result.Error);
            Assert.Contains(to.ToString(), result.Error);
            // The stored status must be untouched by the rejected transition.
            Assert.Equal(from, stored.Status);
        }
    }
}
