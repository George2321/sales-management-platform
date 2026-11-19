using System.Data.Common;
using Microsoft.Data.Sqlite;
using SalesManagementPlatform.Data;

namespace SalesManagementPlatform.Infrastructure;

public sealed class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            ForeignKeys = true
        }.ToString();
    }

    public DbConnection CreateConnection() => new SqliteConnection(_connectionString);
}
