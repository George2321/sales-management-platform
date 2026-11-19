using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Domain.Exceptions;

public sealed class OrderStatusTransitionException : Exception
{
    public OrderStatusTransitionException(OrderStatus currentStatus)
        : base($"This order is already {currentStatus.ToString().ToLowerInvariant()}. Its status can no longer be changed.")
    {
    }
}
