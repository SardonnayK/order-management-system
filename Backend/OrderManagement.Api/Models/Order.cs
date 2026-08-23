namespace OrderManagement.Api.Models;

public class Order
{
    public Guid Id { get; set; }
    public string ClientReference { get; set; } = string.Empty;
    public Customer Customer { get; set; } = default!;
    public List<LineItem> Items { get; set; } = new();
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string Currency { get; set; } = "USD";
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public decimal Subtotal => Items.Sum(item => item.LineTotal);
    public decimal Total => Subtotal;
}
