using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Application.Orders.Dtos;

public sealed record OrderListItemDto(
    Guid Id,
    string ClientProvidedExternalReference,
    DateTime OrderDateTimestamp,
    OrderStatus Status,
    int LineItemCount,
    string Currency,
    decimal Subtotal,
    decimal Total);
