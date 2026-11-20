using Microsoft.Extensions.Configuration;
using SalesManagementPlatform.Application.Features.Orders.Interfaces;
using SalesManagementPlatform.Infrastructure.Persistence;
using SalesManagementPlatform.Infrastructure.Persistence.Database;
using SalesManagementPlatform.Infrastructure.Persistence.Interfaces;
using SalesManagementPlatform.Infrastructure.Persistence.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseAndRepositories(
        this IServiceCollection services, IConfiguration configuration, string contentRootPath)
    {
        var sqlitePath = configuration["Database:SqlitePath"];
        if (string.IsNullOrWhiteSpace(sqlitePath))
        {
            throw new InvalidOperationException("Configure Database:SqlitePath in the API settings.");
        }

        var databasePath = Path.GetFullPath(sqlitePath, contentRootPath);
        services.AddSingleton<IDbConnectionFactory>(_ =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
            return new SqliteConnectionFactory(databasePath);
        });
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddSingleton<SqliteDatabaseInitializer>();

        return services;
    }

    public static Task InitializeDatabaseAsync(this IServiceProvider services, bool seedDemoData)
    {
        return services.GetRequiredService<SqliteDatabaseInitializer>().InitializeAsync(seedDemoData);
    }
}
