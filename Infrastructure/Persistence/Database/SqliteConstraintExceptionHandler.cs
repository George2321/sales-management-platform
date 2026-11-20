using System.Data.Common;
using Dapper;
using Microsoft.Data.Sqlite;
using SalesManagementPlatform.Application.Exceptions;

namespace SalesManagementPlatform.Infrastructure.Persistence.Database;

internal static class SqliteConstraintExceptionHandler
{
    public static async Task<int> ExecuteAsync(
        DbConnection connection, CommandDefinition command, IReadOnlyDictionary<string, string> checkMessages)
    {
        try
        {
            return await connection.ExecuteAsync(command);
        }
        catch (SqliteException error) when (error.SqliteExtendedErrorCode == SQLitePCL.raw.SQLITE_CONSTRAINT_CHECK)
        {
            // SQLite identifies an unnamed CHECK by its expression from Schema.sql.
            foreach (var (expression, message) in checkMessages)
            {
                if (error.Message.Contains($"CHECK constraint failed: {expression}'", StringComparison.Ordinal))
                {
                    throw ApplicationFailureException.Validation(message);
                }
            }

            throw;
        }
    }

}
