using System.Data.Common;
using Dapper;
using SalesManagementPlatform.Application.Orders;
using SalesManagementPlatform.Application.Orders.Dtos;
using SalesManagementPlatform.Application.Orders.Exceptions;
using SalesManagementPlatform.Domain.Entities;
using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Data.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<OrderListItemDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                orders.Id,
                orders.ClientProvidedExternalReference,
                orders.OrderDateTimestamp,
                orders.Status,
                orders.Currency,
                items.Quantity,
                items.UnitPrice
            FROM Orders AS orders
            LEFT JOIN LineItems AS items ON items.OrderId = orders.Id
            ORDER BY orders.OrderDateTimestamp DESC, orders.Id DESC;
            """;

        var orders = new List<OrderListItemDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var hasRow = await reader.ReadAsync(cancellationToken);
        while (hasRow)
        {
            var storedId = reader.GetString(0);
            var id = reader.GetGuid(0);
            var reference = reader.GetString(1);
            var orderDate = DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc);
            var status = (OrderStatus)reader.GetInt32(3);
            var currency = reader.GetString(4);
            var lineItemCount = 0;
            var subtotal = 0m;

            do
            {
                if (!reader.IsDBNull(5))
                {
                    lineItemCount++;
                    subtotal += reader.GetInt32(5) * reader.GetDecimal(6);
                }
                hasRow = await reader.ReadAsync(cancellationToken);
            }
            while (hasRow && reader.GetString(0) == storedId);

            var totals = OrderTotals.FromSubtotal(subtotal);
            orders.Add(new OrderListItemDto(
                Id: id,
                ClientProvidedExternalReference: reference,
                OrderDateTimestamp: orderDate,
                Status: status,
                LineItemCount: lineItemCount,
                Currency: currency,
                Subtotal: totals.Subtotal,
                Total: totals.Total));
        }

        return orders;
    }

    public async Task<OrderDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        return await ReadDetailsAsync(connection, id, null, cancellationToken);
    }

    public async Task<OrderDetailsDto> CreateAsync(Order order, CancellationToken cancellationToken)
    {
        const string insertOrder = """
            INSERT INTO Orders (Id, ClientProvidedExternalReference, OrderDateTimestamp, Status, Currency, Notes)
            VALUES (@Id, @ClientProvidedExternalReference, @OrderDateTimestamp, @Status, @Currency, @Notes)
            ON CONFLICT (ClientProvidedExternalReference) DO NOTHING;
            """;
        const string insertLineItem = """
            INSERT INTO LineItems (Id, OrderId, Sku, Name, Quantity, UnitPrice)
            VALUES (@Id, @OrderId, @Sku, @Name, @Quantity, @UnitPrice);
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var orderId = order.Id.ToString();
        var inserted = await connection.ExecuteAsync(new CommandDefinition(insertOrder, new
        {
            Id = orderId,
            order.ClientProvidedExternalReference,
            order.OrderDateTimestamp,
            Status = (int)order.Status,
            order.Currency,
            order.Notes
        }, transaction, cancellationToken: cancellationToken));

        if (inserted == 0)
        {
            throw new DuplicateOrderReferenceException();
        }

        var lineItems = order.LineItems.Select(item => new
        {
            Id = item.Id.ToString(),
            OrderId = orderId,
            item.Sku,
            item.Name,
            item.Quantity,
            item.UnitPrice
        });
        await connection.ExecuteAsync(new CommandDefinition(
            insertLineItem, lineItems, transaction, cancellationToken: cancellationToken));

        var details = await ReadDetailsAsync(connection, order.Id, transaction, cancellationToken)
            ?? throw new InvalidOperationException("The newly inserted order could not be read.");
        await transaction.CommitAsync(cancellationToken);
        return details;
    }

    public async Task<bool> TryUpdateStatusAsync(
        Guid id, OrderStatus expectedStatus, OrderStatus newStatus, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE Orders
            SET Status = @Status
            WHERE Id = @Id COLLATE NOCASE AND Status = @ExpectedStatus;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            sql,
            new { Id = id.ToString(), ExpectedStatus = (int)expectedStatus, Status = (int)newStatus },
            cancellationToken: cancellationToken);
        return await connection.ExecuteAsync(command) == 1;
    }

    private static async Task<OrderDetailsDto?> ReadDetailsAsync(
        DbConnection connection, Guid id, DbTransaction? transaction, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                orders.Id,
                orders.ClientProvidedExternalReference,
                orders.OrderDateTimestamp,
                orders.CompletedDateTimeStamp,
                orders.Status,
                orders.Currency,
                orders.Notes,
                items.Id,
                items.Sku,
                items.Name,
                items.Quantity,
                items.UnitPrice
            FROM Orders AS orders
            LEFT JOIN LineItems AS items ON items.OrderId = orders.Id
            WHERE orders.Id = @Id COLLATE NOCASE
            ORDER BY items.rowid;
            """;

        await using var reader = await connection.ExecuteReaderAsync(new CommandDefinition(
            sql, new { Id = id.ToString() }, transaction, cancellationToken: cancellationToken));
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var lineItems = new List<OrderLineItemDto>();
        var order = new OrderDetailsDto(
            Id: reader.GetGuid(0),
            ClientProvidedExternalReference: reader.GetString(1),
            OrderDateTimestamp: DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc),
            CompletedDateTimeStamp: reader.IsDBNull(3) ? null
                : DateTime.SpecifyKind(reader.GetDateTime(3), DateTimeKind.Utc),
            Status: (OrderStatus)reader.GetInt32(4),
            Currency: reader.GetString(5),
            Notes: reader.GetString(6),
            LineItems: lineItems);

        do
        {
            if (!reader.IsDBNull(7))
            {
                lineItems.Add(new OrderLineItemDto(
                    Id: reader.GetGuid(7),
                    Sku: reader.GetString(8),
                    Name: reader.GetString(9),
                    Quantity: reader.GetInt32(10),
                    UnitPrice: reader.GetDecimal(11)));
            }
        }
        while (await reader.ReadAsync(cancellationToken));

        return order;
    }
}
