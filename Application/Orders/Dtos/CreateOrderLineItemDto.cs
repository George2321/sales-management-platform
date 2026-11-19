namespace SalesManagementPlatform.Application.Orders.Dtos;

public sealed record CreateOrderLineItemDto(string? Sku, string? Name, int Quantity, decimal UnitPrice);
