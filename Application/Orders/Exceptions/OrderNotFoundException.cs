namespace SalesManagementPlatform.Application.Orders.Exceptions;

public sealed class OrderNotFoundException : Exception
{
    public OrderNotFoundException() : base("This purchase order could not be found.")
    {
    }
}
