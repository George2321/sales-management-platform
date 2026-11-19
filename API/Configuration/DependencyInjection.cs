using SalesManagementPlatform.Data;
using SalesManagementPlatform.Data.Database;
using SalesManagementPlatform.Data.Repositories;
using SalesManagementPlatform.Infrastructure;

namespace SalesManagementPlatform.Api.Configuration;

internal static class DependencyInjection
{
    public static IServiceCollection AddDatabaseAndRepositories(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var sqlitePath = configuration["Database:SqlitePath"]
            ?? throw new InvalidOperationException("Configure Database:SqlitePath in the API settings.");
        var databasePath = Path.GetFullPath(sqlitePath, environment.ContentRootPath);

        services.AddSingleton<IDbConnectionFactory>(_ =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
            return new SqliteConnectionFactory(databasePath);
        });
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddSingleton<SqliteDatabaseInitializer>();

        return services;
    }
}
