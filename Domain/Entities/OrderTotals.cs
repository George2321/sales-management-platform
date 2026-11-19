using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesManagementPlatform.Domain.Entities
{
    public sealed class OrderTotals
    {
        public decimal Total { get; set; }
        public decimal Subtotal { get; set; }
    }
}