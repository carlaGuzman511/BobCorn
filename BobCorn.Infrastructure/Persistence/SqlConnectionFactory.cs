using BobCorn.Application.Abstractions.Persistence;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace BobCorn.Infrastructure.Adapters
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default");
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}
