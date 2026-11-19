namespace SalesManagementPlatform.Application.Orders.Dtos;

public sealed record OrderLineItemDto(Guid Id, string Sku, string Name, int Quantity, decimal UnitPrice);
