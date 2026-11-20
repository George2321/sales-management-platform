using SalesManagementPlatform.Domain.Exceptions;
using SalesManagementPlatform.Domain.ValueObjects;
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
        public Guid CustomerId { get; }
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
            string notes,
            Guid customerId)
        {
            var reference = clientProvidedExternalReference?.Trim() ?? string.Empty;
            if (reference.Length == 0)
            {
                throw new DomainRuleException("Enter an order reference.");
            }

            var currencyCode = currency?.Trim().ToUpperInvariant() ?? string.Empty;
            if (currencyCode.Length != 3 || !currencyCode.All(char.IsAsciiLetter))
            {
                throw new DomainRuleException("Enter a three-letter currency code.");
            }

            if (customerId == Guid.Empty)
            {
                throw new DomainRuleException("Select a customer for the order.");
            }

            if ((status == OrderStatus.Fulfilled) != completedDateTimeStamp.HasValue)
            {
                throw new DomainRuleException("A completion time is required only for fulfilled orders.");
            }

            CustomerId = customerId;
            ClientProvidedExternalReference = reference;
            OrderDateTimestamp = orderDateTimestamp;
            CompletedDateTimeStamp = completedDateTimeStamp;
            Status = status;
            Currency = currencyCode;
            Notes = notes;
        }

        public void ChangeOrderStatus(OrderStatus status)
        {
            (Status, CompletedDateTimeStamp) = TransitionStatus(Status, status);
        }

        public static (OrderStatus Status, DateTime? CompletedDateTimeStamp) TransitionStatus(
            OrderStatus currentStatus, OrderStatus requestedStatus)
        {
            if (!CanChangeStatus(currentStatus))
            {
                throw new DomainRuleException(
                    $"This order is already {currentStatus.ToString().ToLowerInvariant()}. Its status can no longer be changed.");
            }

            return (requestedStatus, requestedStatus == OrderStatus.Fulfilled ? DateTime.UtcNow : null);
        }

        public static bool CanChangeStatus(OrderStatus currentStatus)
        {
            return currentStatus is not (OrderStatus.Fulfilled or OrderStatus.Cancelled);
        }

        public void ChangeOrderDate(DateTime orderDateTimestamp)
        {
            OrderDateTimestamp = orderDateTimestamp;
        }

        public OrderTotals CalculateOrderTotals()
        {
            return OrderTotals.FromSubtotal(LineItems.Sum(item => item.LineTotal));
        }
    }
}
