using SalesManagementPlatformDomain.ValueTypes;

namespace SalesManagementPlatform.Domain
{
    public sealed class Order
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string  ClientProvidedExternalReference { get; }
        public  DateTime Timestamps { get; }
        public OrderStatus Status { get; private set; }
        public string Currency { get;  }
        public string Notes { get;  }

        public Order(string clientProvidedExternalReference, DateTime timestamps, OrderStatus status, string currency, string notes)
        {
            ClientProvidedExternalReference = clientProvidedExternalReference;
            Timestamps = timestamps;
            Status = status;
            Currency = currency;
            Notes = notes;
        }

        public void ChangeOrderStatus(OrderStatus status)
        {
            if (Status == OrderStatus.Fulfilled || Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Cannot change the status of a cancelled order.");
            }
            
            Status = status;
        }
        
    }
}