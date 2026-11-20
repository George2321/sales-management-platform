namespace SalesManagementPlatform.Application.Features.Orders.Dtos;

public sealed record CreateOrderLineItemDto(string? Sku, string? Name, int Quantity, decimal UnitPrice);
