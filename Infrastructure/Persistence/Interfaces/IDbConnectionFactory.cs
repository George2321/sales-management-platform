using System.Data.Common;

namespace SalesManagementPlatform.Infrastructure.Persistence.Interfaces;

internal interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}
