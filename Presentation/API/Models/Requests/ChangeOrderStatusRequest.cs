using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Api.Models.Requests;

public sealed record ChangeOrderStatusRequest(OrderStatus? ExpectedStatus, OrderStatus? Status);
