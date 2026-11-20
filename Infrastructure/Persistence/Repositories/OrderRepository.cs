using System.Data.Common;
using Dapper;
using SalesManagementPlatform.Application.Exceptions;
using SalesManagementPlatform.Application.Features.Orders.Dtos;
using SalesManagementPlatform.Application.Features.Orders.Interfaces;
using SalesManagementPlatform.Infrastructure.Persistence.Database;
using SalesManagementPlatform.Infrastructure.Persistence.Interfaces;
using SalesManagementPlatform.Domain.Entities;
using SalesManagementPlatform.Domain.ValueObjects;
using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository : IOrderRepository
{
    private static readonly TextLimit ReferenceLimit = new("ClientProvidedExternalReference", "Order reference", 128);
    private static readonly TextLimit SkuLimit = new("Sku", "Line item SKU", 64);
    private static readonly TextLimit ItemNameLimit = new("Name", "Line item name", 200);

    private static readonly IReadOnlyDictionary<string, string> OrderCheckMessages = new Dictionary<string, string>
    {
        [ReferenceLimit.CheckExpression] = ReferenceLimit.ErrorMessage,
        ["Status IN (0, 1, 2, 3)"] = "Choose a valid order status."
    };

    private static readonly IReadOnlyDictionary<string, string> LineItemCheckMessages = new Dictionary<string, string>
    {
        [SkuLimit.CheckExpression] = SkuLimit.ErrorMessage,
        [ItemNameLimit.CheckExpression] = ItemNameLimit.ErrorMessage
    };

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
                customers.Name,
                items.Quantity,
                items.UnitPrice
            FROM Orders AS orders
            INNER JOIN Customers AS customers ON customers.Id = orders.CustomerId
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
            var customerName = reader.GetString(5);
            var lineItemCount = 0;
            var subtotal = 0m;

            do
            {
                if (!reader.IsDBNull(6))
                {
                    lineItemCount++;
                    subtotal += LineItem.CalculateLineTotal(reader.GetInt32(6), reader.GetDecimal(7));
                }
                hasRow = await reader.ReadAsync(cancellationToken);
            }
            while (hasRow && reader.GetString(0) == storedId);

            var totals = OrderTotals.FromSubtotal(subtotal);
            orders.Add(new OrderListItemDto(
                Id: id,
                ClientProvidedExternalReference: reference,
                CustomerName: customerName,
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

    public async Task<IEnumerable<OrderCustomerOptionDto>> GetCustomersAsync(CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name FROM Customers ORDER BY Name COLLATE NOCASE, Id;";

        var customers = new List<OrderCustomerOptionDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            customers.Add(new OrderCustomerOptionDto(reader.GetGuid(0), reader.GetString(1)));
        }

        return customers;
    }

    public async Task<OrderDetailsDto> CreateAsync(Order order, CancellationToken cancellationToken)
    {
        const string insertOrder = """
            INSERT INTO Orders (Id, ClientProvidedExternalReference, OrderDateTimestamp, CompletedDateTimeStamp, Status, Currency, Notes, CustomerId)
            VALUES (@Id, @ClientProvidedExternalReference, @OrderDateTimestamp, @CompletedDateTimeStamp, @Status, @Currency, @Notes, @CustomerId)
            ON CONFLICT (ClientProvidedExternalReference) DO NOTHING;
            """;
        const string insertLineItem = """
            INSERT INTO LineItems (Id, OrderId, Sku, Name, Quantity, UnitPrice)
            VALUES (@Id, @OrderId, @Sku, @Name, @Quantity, @UnitPrice);
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var customerId = await connection.QuerySingleOrDefaultAsync<string>(new CommandDefinition(
            "SELECT Id FROM Customers WHERE Id = @Id COLLATE NOCASE;",
            new { Id = order.CustomerId.ToString() }, transaction, cancellationToken: cancellationToken));
        if (customerId is null)
        {
            throw ApplicationFailureException.Validation("The selected customer could not be found. Choose an existing customer.");
        }

        var orderId = order.Id.ToString();
        var orderCommand = new CommandDefinition(insertOrder, new
        {
            Id = orderId,
            ClientProvidedExternalReference = ReferenceLimit.ValidateLength(order.ClientProvidedExternalReference),
            order.OrderDateTimestamp,
            order.CompletedDateTimeStamp,
            Status = (int)order.Status,
            order.Currency,
            order.Notes,
            CustomerId = customerId
        }, transaction, cancellationToken: cancellationToken);
        var inserted = await SqliteConstraintExceptionHandler.ExecuteAsync(connection, orderCommand, OrderCheckMessages);

        if (inserted == 0)
        {
            throw new ApplicationFailureException(
                ApplicationFailureKind.Conflict,
                "Order reference already exists",
                "An order with this reference already exists. Enter a unique reference and try again.");
        }

        var lineItems = order.LineItems.Select(item => new
        {
            Id = item.Id.ToString(),
            OrderId = orderId,
            Sku = SkuLimit.ValidateLength(item.Sku),
            Name = ItemNameLimit.ValidateLength(item.Name),
            item.Quantity,
            item.UnitPrice
        });
        var lineItemCommand = new CommandDefinition(insertLineItem, lineItems, transaction, cancellationToken: cancellationToken);
        await SqliteConstraintExceptionHandler.ExecuteAsync(connection, lineItemCommand, LineItemCheckMessages);

        var details = await ReadDetailsAsync(connection, order.Id, transaction, cancellationToken)
            ?? throw new InvalidOperationException("The newly inserted order could not be read.");
        await transaction.CommitAsync(cancellationToken);
        return details;
    }

    public async Task<bool> TryUpdateStatusAsync(
        Guid id, OrderStatus expectedStatus, OrderStatus newStatus,
        DateTime? completedDateTimeStamp, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE Orders
            SET Status = @Status, CompletedDateTimeStamp = @CompletedDateTimeStamp
            WHERE Id = @Id COLLATE NOCASE AND Status = @ExpectedStatus;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            sql,
            new
            {
                Id = id.ToString(),
                ExpectedStatus = (int)expectedStatus,
                Status = (int)newStatus,
                CompletedDateTimeStamp = completedDateTimeStamp
            },
            cancellationToken: cancellationToken);
        return await SqliteConstraintExceptionHandler.ExecuteAsync(connection, command, OrderCheckMessages) == 1;
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
                customers.Name,
                items.Id,
                items.Sku,
                items.Name,
                items.Quantity,
                items.UnitPrice
            FROM Orders AS orders
            INNER JOIN Customers AS customers ON customers.Id = orders.CustomerId
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

        var orderId = reader.GetGuid(0);
        var reference = reader.GetString(1);
        var orderDate = DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc);
        DateTime? completedDate = reader.IsDBNull(3) ? null
            : DateTime.SpecifyKind(reader.GetDateTime(3), DateTimeKind.Utc);
        var status = (OrderStatus)reader.GetInt32(4);
        var currency = reader.GetString(5);
        var notes = reader.GetString(6);
        var customerName = reader.GetString(7);
        var lineItems = new List<OrderLineItemDto>();

        do
        {
            if (!reader.IsDBNull(8))
            {
                var quantity = reader.GetInt32(11);
                var unitPrice = reader.GetDecimal(12);
                lineItems.Add(new OrderLineItemDto(
                    Id: reader.GetGuid(8),
                    Sku: reader.GetString(9),
                    Name: reader.GetString(10),
                    Quantity: quantity,
                    UnitPrice: unitPrice,
                    LineTotal: LineItem.CalculateLineTotal(quantity, unitPrice)));
            }
        }
        while (await reader.ReadAsync(cancellationToken));

        var totals = OrderTotals.FromSubtotal(lineItems.Sum(item => item.LineTotal));
        return new OrderDetailsDto(orderId, reference, customerName, orderDate, completedDate,
            status, currency, notes, totals.Subtotal, totals.Total, lineItems);
    }

    private sealed record TextLimit(string Column, string Label, int MaxLength)
    {
        public string CheckExpression => $"length({Column}) <= {MaxLength}";
        public string ErrorMessage => $"{Label} must be {MaxLength} characters or fewer.";

        public string ValidateLength(string value)
        {
            // SQLite counts a surrogate pair as one character and stops at an embedded NUL.
            if (value.Length > MaxLength && (value.Contains('\0') || value.Any(char.IsSurrogate)))
            {
                throw ApplicationFailureException.Validation(ErrorMessage);
            }

            return value;
        }
    }
}
