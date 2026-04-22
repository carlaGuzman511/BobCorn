namespace BobCorn.Application.Abstractions.Persistence
{
    public interface ICornPurchaseRepository
    {
        Task<(bool Success, DateTimeOffset NextAllowedAt)> TryPurchaseAsync(string clientId);
        Task<int> GetTotalPurchasesAsync(string clientId);
    }
}
