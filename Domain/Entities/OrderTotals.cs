namespace SalesManagementPlatform.Domain.Entities
{
    public sealed class OrderTotals
    {
        public decimal Total { get; set; }
        public decimal Subtotal { get; set; }

        public static OrderTotals FromSubtotal(decimal subtotal)
        {
            return new OrderTotals
            {
                Subtotal = subtotal,
                Total = subtotal + (subtotal * 0.1m)
            };
        }
    }
}
