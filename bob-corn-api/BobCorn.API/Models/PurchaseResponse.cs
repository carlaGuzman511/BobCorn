namespace BobCorn.API.Models
{
    public class PurchaseResponse
    {
        public string Message { get; }
        public int TotalPurchased { get; }
        public DateTimeOffset? NextAllowedAt { get; }

        public PurchaseResponse(string message, int totalPurchased, DateTimeOffset? nextAllowedAt)
        {
            Message = message;
            TotalPurchased = totalPurchased;
            NextAllowedAt = nextAllowedAt;
        }
    }
}