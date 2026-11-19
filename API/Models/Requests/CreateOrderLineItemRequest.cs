namespace SalesManagementPlatform.Api.Models.Requests;

public sealed record CreateOrderLineItemRequest(string? Sku, string? Name, int Quantity, decimal UnitPrice);
