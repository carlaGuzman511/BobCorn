namespace BobCorn.Application.Abstractions.Persistence
{
    public interface ICornPurchaseRepository
    {
        Task<int> GetTotalPurchasesAsync(string clientId);
        Task<DateTimeOffset?> GetLastPurchaseAsync(string clientId);
        Task SetLastPurchaseAsync(string clientId, DateTimeOffset time);
    }
}
