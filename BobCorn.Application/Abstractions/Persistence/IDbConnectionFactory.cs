using System.Data;

namespace BobCorn.Application.Abstractions.Persistence
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
