using SalesManagementPlatform.Application.Features.Orders.Dtos;
using SalesManagementPlatform.Domain.Entities;
using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Application.Features.Orders.Interfaces;

public interface IOrderRepository
{
    Task<IEnumerable<OrderListItemDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<OrderDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<OrderCustomerOptionDto>> GetCustomersAsync(CancellationToken cancellationToken);

    Task<OrderDetailsDto> CreateAsync(Order order, CancellationToken cancellationToken);

    Task<bool> TryUpdateStatusAsync(
        Guid id, OrderStatus expectedStatus, OrderStatus newStatus,
        DateTime? completedDateTimeStamp, CancellationToken cancellationToken);
}
