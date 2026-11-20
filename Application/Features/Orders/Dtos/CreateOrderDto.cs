namespace SalesManagementPlatform.Application.Features.Orders.Dtos;

public sealed record CreateOrderDto(
    string? ClientProvidedExternalReference,
    Guid? CustomerId,
    string? Currency,
    string? Notes,
    IReadOnlyList<CreateOrderLineItemDto?>? LineItems);
