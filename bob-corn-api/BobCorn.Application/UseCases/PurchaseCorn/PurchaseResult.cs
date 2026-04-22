namespace BobCorn.Application.UseCases.PurchaseCorn
{
    public class PurchaseResult
    {
        public bool IsSuccess { get; }
        public DateTimeOffset? NextAllowedAt { get; }
        public int TotalPurchased { get; }

        private PurchaseResult(bool isSuccess, DateTimeOffset? nextAllowedAt, int totalPurchased)
        {
            IsSuccess = isSuccess;
            NextAllowedAt = nextAllowedAt;
            TotalPurchased = totalPurchased;
        }

        public static PurchaseResult Success(DateTimeOffset nextAllowedAt, int total)
            => new(true, nextAllowedAt, total);

        public static PurchaseResult Failure(DateTimeOffset nextAllowedAt, int total)
            => new(false, nextAllowedAt, total);
    }
}
