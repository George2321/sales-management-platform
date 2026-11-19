using SalesManagementPlatform.Application.Orders.Dtos;

namespace SalesManagementPlatform.Application.Orders;

public interface IOrderService
{
    Task<IReadOnlyList<OrderListItemDto>> GetAllAsync(CancellationToken cancellationToken);
}
