using SalesManagementPlatform.Domain.Exceptions;
using SalesManagementPlatform.Domain.ValueTypes;

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
        public OrderTotals OrderTotals => CalculateOrderTotals();

        public List<LineItem> LineItems = new List<LineItem>();

        public Order(
            string? clientProvidedExternalReference,
            DateTime orderDateTimestamp,
            DateTime? completedDateTimeStamp,
            OrderStatus status,
            string? currency,
            string notes)
        {
            var reference = clientProvidedExternalReference?.Trim() ?? string.Empty;
            if (reference.Length is 0 or > 128)
            {
                throw new DomainRuleException("Enter an order reference of 1 to 128 characters.");
            }

            var currencyCode = currency?.Trim().ToUpperInvariant() ?? string.Empty;
            if (currencyCode.Length != 3 || !currencyCode.All(char.IsAsciiLetter))
            {
                throw new DomainRuleException("Enter a three-letter currency code.");
            }

            ClientProvidedExternalReference = reference;
            OrderDateTimestamp = orderDateTimestamp;
            CompletedDateTimeStamp = completedDateTimeStamp;
            Status = status;
            Currency = currencyCode;
            Notes = notes;
        }

        public void ChangeOrderStatus(OrderStatus status)
        {
            Status = TransitionStatus(Status, status);
        }

        public static OrderStatus TransitionStatus(OrderStatus currentStatus, OrderStatus requestedStatus)
        {
            if (!Enum.IsDefined(requestedStatus))
            {
                throw new DomainRuleException("Choose a valid order status.");
            }

            if (!CanChangeStatus(currentStatus))
            {
                throw new OrderStatusTransitionException(currentStatus);
            }

            return requestedStatus;
        }

        public static bool CanChangeStatus(OrderStatus currentStatus)
        {
            return currentStatus is not (OrderStatus.Fulfilled or OrderStatus.Cancelled);
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
            return OrderTotals.FromSubtotal(LineItems.Sum(item => item.LineTotal));
        }
    }
}
