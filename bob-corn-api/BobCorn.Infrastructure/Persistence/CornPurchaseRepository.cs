using BobCorn.Application.Abstractions.Persistence;
using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace BobCorn.Infrastructure.Adapters
{
    public class CornPurchaseRepository : ICornPurchaseRepository
    {
        private readonly IDbConnectionFactory _factory;

        public CornPurchaseRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<int> GetTotalPurchasesAsync(string clientId)
        {
            using var connection = _factory.CreateConnection();

            return await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) FROM CornPurchases WHERE ClientId = @ClientId",
                new { ClientId = clientId });
        }

        public async Task<(bool Success, DateTimeOffset NextAllowedAt)> TryPurchaseAsync(string clientId)
        {
            using var connection = _factory.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var now = DateTimeOffset.UtcNow;

                var lastPurchase = await connection.QuerySingleOrDefaultAsync<DateTimeOffset?>(
                    @"SELECT LastPurchaseAt
                      FROM ClientPurchaseState
                      WHERE ClientId = @ClientId",
                    new { ClientId = clientId },
                    transaction);

                if (lastPurchase.HasValue)
                {
                    var nextAllowed = lastPurchase.Value.AddMinutes(1);

                    if (nextAllowed > now)
                    {
                        transaction.Rollback();
                        return (false, nextAllowed);
                    }

                    await connection.ExecuteAsync(
                        @"UPDATE ClientPurchaseState
                          SET LastPurchaseAt = @Now
                          WHERE ClientId = @ClientId",
                        new { ClientId = clientId, Now = now },
                        transaction);
                }
                else
                {
                    await connection.ExecuteAsync(
                        @"INSERT INTO ClientPurchaseState (ClientId, LastPurchaseAt)
                            VALUES (@ClientId, @Now)",
                        new { ClientId = clientId, Now = now },
                        transaction);
                }

                await connection.ExecuteAsync(
                    @"INSERT INTO CornPurchases (Id, ClientId, PurchasedAt)
                        VALUES (@Id, @ClientId, @Now)",
                    new { Id = Guid.NewGuid(), ClientId = clientId, Now = now },
                    transaction);

                transaction.Commit();

                return (true, now.AddMinutes(1));
            }
            catch (SqlException ex)
            {
                transaction.Rollback();

                return (false, DateTimeOffset.UtcNow.AddMinutes(1));
            }
            catch(Exception e)
            {
                transaction.Rollback();

                return (false, DateTimeOffset.UtcNow.AddMinutes(1));
            }
        }
    }
}
