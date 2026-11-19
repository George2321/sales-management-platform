using System.Data.Common;

namespace SalesManagementPlatform.Data;

public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}
