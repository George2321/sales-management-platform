using SalesManagementPlatformDomain.ValueTypes;

namespace SalesManagementPlatform.Domain.Entities
{
    public sealed class Order
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string ClientProvidedExternalReference { get; }
        public DateTime OrderDateTimestamp { get; private set; }
        public DateTime? CompletedDateTimeStamp { get; private set; }
        public OrderStatus Status { get; private set; }
        public string Currency { get; }
        public string Notes { get; set; }
        public Guid CustomerId { get; }
        public OrderTotals OrderTotals => CalculateOrderTotals();

        public List<LineItem> LineItems = new List<LineItem>();

        public Order(
            string clientProvidedExternalReference,
            DateTime orderDateTimestamp,
            DateTime completedDateTimeStamp,
            OrderStatus status,
            string currency,
            string notes,
            Guid customerId)
        {
            ClientProvidedExternalReference = clientProvidedExternalReference;
            OrderDateTimestamp = orderDateTimestamp;
            CompletedDateTimeStamp = completedDateTimeStamp;
            Status = status;
            Currency = currency;
            Notes = notes;
            CustomerId = customerId;
        }

        public void ChangeOrderStatus(OrderStatus status)
        {
            if (Status == OrderStatus.Fulfilled || Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Cannot change the status of a cancelled or fulfilled order.");
            }

            Status = status;
        }

        public void ChangeOrderDate(DateTime orderDateTimestamp)
        {
            OrderDateTimestamp = orderDateTimestamp;
        }

        public void ChangeCompletedDate(DateTime completedDateTimeStamp)
        {
            CompletedDateTimeStamp = completedDateTimeStamp;
        }

        public OrderTotals CalculateOrderTotals()
        {
            OrderTotals.Subtotal = LineItems.Sum(item => item.LineTotal);
            OrderTotals.Total = OrderTotals.Subtotal + (OrderTotals.Subtotal * 0.1m);
            return OrderTotals;
        }
    }
}