using Dapper;
using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Data.Database;

public sealed class SqliteDatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SqliteDatabaseInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync(bool seedDemoData)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(await ReadScriptAsync("Schema.sql"), transaction: transaction);

        if (seedDemoData && !await connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS (SELECT 1 FROM Orders);", transaction: transaction))
        {
            var statuses = new
            {
                Pending = (int)OrderStatus.Pending,
                Confirmed = (int)OrderStatus.Confirmed,
                Fulfilled = (int)OrderStatus.Fulfilled,
                Cancelled = (int)OrderStatus.Cancelled
            };
            await connection.ExecuteAsync(await ReadScriptAsync("DemoSeed.sql"), statuses, transaction);
        }

        await transaction.CommitAsync();
    }

    private static async Task<string> ReadScriptAsync(string fileName)
    {
        var type = typeof(SqliteDatabaseInitializer);
        using var stream = type.Assembly.GetManifestResourceStream(type, fileName)
            ?? throw new InvalidOperationException($"The embedded database script '{fileName}' is missing.");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
