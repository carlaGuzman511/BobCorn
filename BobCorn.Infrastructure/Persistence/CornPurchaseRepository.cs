using BobCorn.Application.Abstractions.Persistence;
using Dapper;

namespace BobCorn.Infrastructure.Adapters
{
    public class CornPurchaseRepository : ICornPurchaseRepository
    {
        private readonly IDbConnectionFactory _factory;

        public CornPurchaseRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<DateTimeOffset?> GetLastPurchaseAsync(string clientId)
        {
            using var connection = _factory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<DateTimeOffset?>(
                @"SELECT LastPurchaseAt 
                  FROM ClientPurchaseState 
                  WHERE ClientId = @ClientId",
                new { ClientId = clientId });


        }


        public async Task SetLastPurchaseAsync(string clientId, DateTimeOffset time)
        {
            using var connection = _factory.CreateConnection();

            var sql = @"
                IF EXISTS (SELECT 1 FROM ClientPurchaseState WHERE ClientId = @ClientId)
                    UPDATE ClientPurchaseState
                    SET LastPurchaseAt = @Time
                    WHERE ClientId = @ClientId
                ELSE
                    INSERT INTO ClientPurchaseState (ClientId, LastPurchaseAt)
                    VALUES (@ClientId, @Time)";

            await connection.ExecuteAsync(sql, new { ClientId = clientId, Time = time });

            await connection.ExecuteAsync(
                @"INSERT INTO CornPurchases (Id, ClientId, PurchasedAt)
              VALUES (@Id, @ClientId, @Time)",
                new { Id = Guid.NewGuid(), ClientId = clientId, Time = time });
        }

        public async Task<int> GetTotalPurchasesAsync(string clientId)
        {
            using var connection = _factory.CreateConnection();

            return await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) FROM CornPurchases WHERE ClientId = @ClientId",
                new { ClientId = clientId });
        }
    }
}
