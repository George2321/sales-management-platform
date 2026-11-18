using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesManagementPlatform.Domain
{
    public class LineItem
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Sku { get;  }
        public string Name { get;  }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal LineTotal => Quantity * UnitPrice;    

        public LineItem(string sku, string name, int quantity, decimal unitPrice)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Name = name.Trim();
            Sku = sku;
            Name = name;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }    

        public void ChangeQuantity(int quantity)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
            Quantity = quantity;
        }

         public void ChangeUnitPrice(decimal price)
         {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);
            UnitPrice = price;
         }
    } 
}