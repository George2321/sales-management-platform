namespace SalesManagementPlatform.Application.Orders.Exceptions;

public sealed class OrderStatusConflictException : Exception
{
    public OrderStatusConflictException()
        : base("The order status changed after you opened it. Your change was not saved.")
    {
    }
}
