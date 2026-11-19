namespace SalesManagementPlatform.Domain.Exceptions;

/// <summary>A domain rule violation with a message suitable for users.</summary>
public class DomainRuleException : Exception
{
    public DomainRuleException(string message) : base(message)
    {
    }
}
