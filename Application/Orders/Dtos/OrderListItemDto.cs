using SalesManagementPlatformDomain.ValueTypes;

namespace SalesManagementPlatform.Application.Orders.Dtos;

public sealed record OrderListItemDto(
    Guid Id,
    string ClientProvidedExternalReference,
    string CustomerName,
    DateTime OrderDateTimestamp,
    OrderStatus Status,
    int LineItemCount);
