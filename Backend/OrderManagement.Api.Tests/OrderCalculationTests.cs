using OrderManagement.Api.Models;

namespace OrderManagement.Api.Tests;

public class OrderCalculationTests
{
    [Fact]
    public void SubtotalAndTotal_AreComputedFromLineItems()
    {
        // 2 x R10.00 + 1 x R5.00 = R25.00
        var order = new Order
        {
            Currency = "ZAR",
            Items =
            [
                new LineItem { Sku = "SKU-A", Name = "Widget", Quantity = 2, UnitPrice = 10.00m },
                new LineItem { Sku = "SKU-B", Name = "Gadget", Quantity = 1, UnitPrice = 5.00m },
            ],
        };

        Assert.Equal(25.00m, order.Subtotal);
        Assert.Equal(25.00m, order.Total);
    }
}
