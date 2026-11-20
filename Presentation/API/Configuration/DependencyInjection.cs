using SalesManagementPlatform.Application.Features.Orders;
using SalesManagementPlatform.Application.Features.Orders.Interfaces;

namespace SalesManagementPlatform.Api.Configuration;

internal static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services.AddScoped<IOrderService, OrderService>();
    }
}
