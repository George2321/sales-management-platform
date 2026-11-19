using SalesManagementPlatform.Application.Orders.Dtos;

namespace SalesManagementPlatform.Application.Orders;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public Task<IReadOnlyList<OrderListItemDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _orderRepository.GetAllAsync(cancellationToken);
    }
}
