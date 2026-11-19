namespace SalesManagementPlatform.Application.Orders.Dtos;

public sealed record CreateOrderDto(
    string? ClientProvidedExternalReference,
    string? Currency,
    string? Notes,
    IReadOnlyList<CreateOrderLineItemDto?>? LineItems);
