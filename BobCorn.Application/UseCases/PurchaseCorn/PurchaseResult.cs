namespace BobCorn.Application.UseCases.PurchaseCorn
{
    public class PurchaseResult
    {
        public bool IsSuccess { get; }
        public DateTimeOffset? NextAllowedAt { get; }

        private PurchaseResult(bool isSuccess, DateTimeOffset? nextAllowedAt)
        {
            IsSuccess = isSuccess;
            NextAllowedAt = nextAllowedAt;
        }

        public static PurchaseResult Success(DateTimeOffset nextAllowedAt)
            => new(true, nextAllowedAt);

        public static PurchaseResult Failure(DateTimeOffset nextAllowedAt)
            => new(false, nextAllowedAt);
    }
}
