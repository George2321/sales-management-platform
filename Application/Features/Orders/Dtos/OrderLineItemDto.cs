namespace SalesManagementPlatform.Application.Features.Orders.Dtos;

public sealed record OrderLineItemDto(Guid Id, string Sku, string Name, int Quantity, decimal UnitPrice, decimal LineTotal);
