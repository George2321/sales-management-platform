using SalesManagementPlatform.Application.Orders.Dtos;
using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Application.Orders;

public interface IOrderService
{
    Task<IEnumerable<OrderListItemDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<OrderDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<OrderDetailsDto> CreateAsync(CreateOrderDto request, CancellationToken cancellationToken);

    Task<OrderDetailsDto> ChangeStatusAsync(
        Guid id, OrderStatus expectedStatus, OrderStatus status, CancellationToken cancellationToken);
}
