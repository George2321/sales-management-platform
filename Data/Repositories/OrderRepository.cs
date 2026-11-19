using SalesManagementPlatform.Application.Orders;
using SalesManagementPlatform.Application.Orders.Dtos;
using SalesManagementPlatformDomain.ValueTypes;

namespace SalesManagementPlatform.Data.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<OrderListItemDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                orders.Id,
                orders.ClientProvidedExternalReference,
                customers.Name,
                orders.OrderDateTimestamp,
                orders.Status,
                (
                    SELECT COUNT(*)
                    FROM LineItems AS items
                    WHERE items.OrderId = orders.Id
                ) AS LineItemCount
            FROM Orders AS orders
            INNER JOIN Customers AS customers ON customers.Id = orders.CustomerId
            ORDER BY orders.OrderDateTimestamp DESC, orders.Id DESC;
            """;

        var orders = new List<OrderListItemDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            orders.Add(new OrderListItemDto(
                Id: reader.GetGuid(0),
                ClientProvidedExternalReference: reader.GetString(1),
                CustomerName: reader.GetString(2),
                OrderDateTimestamp: DateTime.SpecifyKind(reader.GetDateTime(3), DateTimeKind.Utc),
                Status: (OrderStatus)reader.GetInt32(4),
                LineItemCount: reader.GetInt32(5)));
        }

        return orders;
    }
}
