using SalesManagementPlatform.Domain.Exceptions;

namespace SalesManagementPlatform.Domain.Entities
{
    public sealed class LineItem
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Sku { get; }
        public string Name { get; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal LineTotal => CalculateLineTotal();

        public LineItem(string? sku, string? name, int quantity, decimal unitPrice)
        {
            var itemSku = sku?.Trim() ?? string.Empty;
            if (itemSku.Length == 0)
            {
                throw new DomainRuleException("Line item SKU is required.");
            }

            var itemName = name?.Trim() ?? string.Empty;
            if (itemName.Length == 0)
            {
                throw new DomainRuleException("Line item name is required.");
            }

            Sku = itemSku;
            Name = itemName;
            ChangeQuantity(quantity);
            ChangeUnitPrice(unitPrice);
        }

        public void ChangeQuantity(int quantity)
        {
            if (quantity <= 0)
            {
                throw new DomainRuleException("Line item quantity must be greater than zero.");
            }

            Quantity = quantity;
        }

        public void ChangeUnitPrice(decimal price)
        {
            if (price < 0)
            {
                throw new DomainRuleException("Line item unit price cannot be negative.");
            }

            UnitPrice = price;
        }

        public decimal CalculateLineTotal()
        {
            return CalculateLineTotal(Quantity, UnitPrice);
        }

        public static decimal CalculateLineTotal(int quantity, decimal unitPrice) => quantity * unitPrice;
    }
}
