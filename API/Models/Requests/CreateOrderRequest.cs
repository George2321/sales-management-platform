namespace SalesManagementPlatform.Api.Models.Requests;

public sealed record CreateOrderRequest(
    string? ClientProvidedExternalReference,
    string? Currency,
    string? Notes,
    IReadOnlyList<CreateOrderLineItemRequest?>? LineItems);
