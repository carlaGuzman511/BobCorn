using BobCorn.Infrastructure.Adapters;
using BobCorn.Infrastructure.Persistence;
using FluentAssertions;

namespace BobCorn.IntegrationTests
{
    public class ConcurrencyTests
    {
        [Fact]
        public async Task Only_One_Request_Should_Succeed_Concurrent()
        {
            var repo = CreateRealRepository();
            var clientId = Guid.NewGuid().ToString();

            var tasks = Enumerable.Range(0, 10)
                .Select(_ => Task.Run(() => repo.TryPurchaseAsync(clientId)))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            results.Count(r => r.Success).Should().Be(1);
            results.Count(r => !r.Success).Should().Be(9);
        }

        private CornPurchaseRepository CreateRealRepository()
        {
            var factory = new TestDbConnectionFactory(
                "Server=localhost;Database=BobCornTest;Trusted_Connection=True;TrustServerCertificate=True");

            return new CornPurchaseRepository(factory);
        }
    }
}
