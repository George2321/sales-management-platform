using SalesManagementPlatform.Domain.Entities;
using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Application.Orders.Dtos;

public sealed record OrderDetailsDto(
    Guid Id,
    string ClientProvidedExternalReference,
    DateTime OrderDateTimestamp,
    DateTime? CompletedDateTimeStamp,
    OrderStatus Status,
    string Currency,
    string Notes,
    IReadOnlyList<OrderLineItemDto> LineItems)
{
    public bool CanChangeStatus => Order.CanChangeStatus(Status);
}
