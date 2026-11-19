using SalesManagementPlatform.Application.Orders.Dtos;
using SalesManagementPlatform.Application.Orders.Exceptions;
using SalesManagementPlatform.Domain.Entities;
using SalesManagementPlatform.Domain.Exceptions;
using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Application.Orders;

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
            ?? throw new OrderNotFoundException();
    }

    public Task<OrderDetailsDto> CreateAsync(CreateOrderDto request, CancellationToken cancellationToken)
    {
        var order = new Order(
            request.ClientProvidedExternalReference, DateTime.UtcNow, null, OrderStatus.Pending,
            request.Currency, request.Notes ?? string.Empty);

        if (request.LineItems is not { Count: > 0 } lineItems)
        {
            throw new DomainRuleException("Add at least one line item.");
        }

        foreach (var item in lineItems)
        {
            if (item is null)
            {
                throw new DomainRuleException("Enter the details for each line item.");
            }

            order.LineItems.Add(new LineItem(item.Sku, item.Name, item.Quantity, item.UnitPrice));
        }

        try
        {
            order.CalculateOrderTotals();
        }
        catch (OverflowException)
        {
            throw new DomainRuleException("The order amount is too large. Enter smaller quantities or prices.");
        }

        return _orderRepository.CreateAsync(order, cancellationToken);
    }

    public async Task<OrderDetailsDto> ChangeStatusAsync(
        Guid id, OrderStatus expectedStatus, OrderStatus status, CancellationToken cancellationToken)
    {
        var order = await GetByIdAsync(id, cancellationToken);
        var newStatus = Order.TransitionStatus(order.Status, status);

        if (order.Status != expectedStatus ||
            !await _orderRepository.TryUpdateStatusAsync(order.Id, expectedStatus, newStatus, cancellationToken))
        {
            throw new OrderStatusConflictException();
        }

        return order with { Status = newStatus };
    }
}
