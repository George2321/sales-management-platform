using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Application.Features.Orders.Dtos;

public sealed record OrderListItemDto(
    Guid Id,
    string ClientProvidedExternalReference,
    string CustomerName,
    DateTime OrderDateTimestamp,
    OrderStatus Status,
    int LineItemCount,
    string Currency,
    decimal Subtotal,
    decimal Total);
