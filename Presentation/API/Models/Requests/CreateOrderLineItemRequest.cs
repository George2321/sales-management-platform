using System.Text.Json.Serialization;

namespace SalesManagementPlatform.Api.Models.Requests;

public sealed record CreateOrderLineItemRequest(string? Sku, string? Name, int Quantity, [property: JsonRequired] decimal UnitPrice);
