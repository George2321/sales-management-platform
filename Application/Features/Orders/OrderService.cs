using SalesManagementPlatform.Application.Exceptions;
using SalesManagementPlatform.Application.Features.Orders.Dtos;
using SalesManagementPlatform.Application.Features.Orders.Interfaces;
using SalesManagementPlatform.Domain.Entities;
using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Application.Features.Orders;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public Task<IEnumerable<OrderListItemDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _orderRepository.GetAllAsync(cancellationToken);
    }

    public async Task<OrderDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ApplicationFailureException(
                ApplicationFailureKind.NotFound, "Order not found", "This purchase order could not be found.");
    }

    public Task<IEnumerable<OrderCustomerOptionDto>> GetCustomersAsync(CancellationToken cancellationToken)
    {
        return _orderRepository.GetCustomersAsync(cancellationToken);
    }

    public Task<OrderDetailsDto> CreateAsync(CreateOrderDto request, CancellationToken cancellationToken)
    {
        var order = new Order(
            request.ClientProvidedExternalReference, DateTime.UtcNow, null, OrderStatus.Pending,
            request.Currency, request.Notes ?? string.Empty, request.CustomerId.GetValueOrDefault());

        if (request.LineItems is not { Count: > 0 } lineItems)
        {
            throw ApplicationFailureException.Validation("Add at least one line item.");
        }

        foreach (var item in lineItems)
        {
            if (item is null)
            {
                throw ApplicationFailureException.Validation("Enter the details for each line item.");
            }

            order.LineItems.Add(new LineItem(item.Sku, item.Name, item.Quantity, item.UnitPrice));
        }

        try
        {
            order.CalculateOrderTotals();
        }
        catch (OverflowException)
        {
            throw ApplicationFailureException.Validation("The order amount is too large. Enter smaller quantities or prices.");
        }

        return _orderRepository.CreateAsync(order, cancellationToken);
    }

    public async Task<OrderDetailsDto> ChangeStatusAsync(
        Guid id, OrderStatus expectedStatus, OrderStatus status, CancellationToken cancellationToken)
    {
        var order = await GetByIdAsync(id, cancellationToken);
        if (order.Status != expectedStatus)
        {
            throw StatusConflict();
        }

        var change = Order.TransitionStatus(order.Status, status);
        var updated = await _orderRepository.TryUpdateStatusAsync(
            order.Id, expectedStatus, change.Status, change.CompletedDateTimeStamp, cancellationToken);
        if (!updated)
        {
            throw StatusConflict();
        }

        return order with { Status = change.Status, CompletedDateTimeStamp = change.CompletedDateTimeStamp };
    }

    private static ApplicationFailureException StatusConflict() => new(
        ApplicationFailureKind.Conflict,
        "Order status could not be saved",
        "The order status changed after you opened it. Your change was not saved.");
}
