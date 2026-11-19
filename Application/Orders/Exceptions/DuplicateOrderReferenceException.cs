namespace SalesManagementPlatform.Application.Orders.Exceptions;

public sealed class DuplicateOrderReferenceException : Exception
{
    public DuplicateOrderReferenceException()
        : base("An order with this reference already exists. Enter a unique reference and try again.")
    {
    }
}
