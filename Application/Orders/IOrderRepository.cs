using SalesManagementPlatform.Application.Orders.Dtos;
using SalesManagementPlatform.Domain.Entities;
using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Application.Orders;

public interface IOrderRepository
{
    Task<IEnumerable<OrderListItemDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<OrderDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<OrderDetailsDto> CreateAsync(Order order, CancellationToken cancellationToken);

    Task<bool> TryUpdateStatusAsync(
        Guid id, OrderStatus expectedStatus, OrderStatus newStatus, CancellationToken cancellationToken);
}
