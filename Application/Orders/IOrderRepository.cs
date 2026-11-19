using SalesManagementPlatform.Application.Orders.Dtos;

namespace SalesManagementPlatform.Application.Orders;

public interface IOrderRepository
{
    Task<IReadOnlyList<OrderListItemDto>> GetAllAsync(CancellationToken cancellationToken);
}
