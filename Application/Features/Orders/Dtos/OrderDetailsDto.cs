using SalesManagementPlatform.Domain.Entities;
using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Application.Features.Orders.Dtos;

public sealed record OrderDetailsDto(
    Guid Id,
    string ClientProvidedExternalReference,
    string CustomerName,
    DateTime OrderDateTimestamp,
    DateTime? CompletedDateTimeStamp,
    OrderStatus Status,
    string Currency,
    string Notes,
    IReadOnlyList<OrderLineItemDto> LineItems)
{
    public bool CanChangeStatus => Order.CanChangeStatus(Status);
}
