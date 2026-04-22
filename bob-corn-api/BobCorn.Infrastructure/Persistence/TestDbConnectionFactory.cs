using System.Data;
using Microsoft.Data.SqlClient;
using BobCorn.Application.Abstractions.Persistence;

namespace BobCorn.Infrastructure.Persistence
{
    public class TestDbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public TestDbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}
