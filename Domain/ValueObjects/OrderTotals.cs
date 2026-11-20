namespace SalesManagementPlatform.Domain.ValueObjects;

public sealed record OrderTotals
{
    public decimal Subtotal { get; }
    public decimal Total { get; }

    private OrderTotals(decimal subtotal)
    {
        Subtotal = subtotal;
        Total = subtotal + (subtotal * 0.1m);
    }

    public static OrderTotals FromSubtotal(decimal subtotal) => new(subtotal);
}
