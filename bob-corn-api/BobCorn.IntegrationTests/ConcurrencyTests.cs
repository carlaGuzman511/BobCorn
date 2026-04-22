using BobCorn.Infrastructure.Adapters;
using BobCorn.Infrastructure.Persistence;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BobCorn.IntegrationTests;

public class ConcurrencyTests : IAsyncLifetime
{
    private const string ConnectionString = "Server=.\\SQLEXPRESS;Database=BobCornTest;Trusted_Connection=True;TrustServerCertificate=True;";
    private CornPurchaseRepository _repository = null!;

    public async Task InitializeAsync()
    {
        _repository = new CornPurchaseRepository(new TestDbConnectionFactory(ConnectionString));
    }

    public async Task DisposeAsync()
    {
        await CleanupAsync();
    }

    [Fact]
    public async Task Only_One_Request_Should_Succeed_For_Same_Client()
    {
        var clientId = Guid.NewGuid().ToString();

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => _repository.TryPurchaseAsync(clientId)))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        results.Count(r => r.Success).Should().Be(1, "Only one purchase should succeed");
        results.Count(r => !r.Success).Should().Be(9, "The other 9 should be rate-limited");

        var successResult = results.First(r => r.Success);
        successResult.NextAllowedAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMinutes(1), TimeSpan.FromSeconds(2));

        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        var purchaseCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM CornPurchases WHERE ClientId = @ClientId", new { ClientId = clientId });
        purchaseCount.Should().Be(1);

        var state = await conn.QuerySingleOrDefaultAsync<DateTimeOffset?>(
            "SELECT LastPurchaseAt FROM ClientPurchaseState WHERE ClientId = @ClientId", new { ClientId = clientId });
        state.Should().NotBeNull();
    }

    [Fact]
    public async Task Different_Clients_Can_Purchase_Concurrently()
    {
        var clientId1 = Guid.NewGuid().ToString();
        var clientId2 = Guid.NewGuid().ToString();

        var task1 = _repository.TryPurchaseAsync(clientId1);
        var task2 = _repository.TryPurchaseAsync(clientId2);
        await Task.WhenAll(task1, task2);

        task1.Result.Success.Should().BeTrue();
        task2.Result.Success.Should().BeTrue();

        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(DISTINCT ClientId) FROM CornPurchases WHERE ClientId IN (@id1, @id2)",
            new { id1 = clientId1, id2 = clientId2 });
        count.Should().Be(2);
    }

    private async Task CleanupAsync()
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM CornPurchases; DELETE FROM ClientPurchaseState;";
        await cmd.ExecuteNonQueryAsync();
    }
}