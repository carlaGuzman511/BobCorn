using BobCorn.Application.Abstractions.Persistence;
using BobCorn.Application.Abstractions.Time;

namespace BobCorn.Application.UseCases.PurchaseCorn
{
    public class PurchaseCornHandler
    {
        private readonly ICornPurchaseRepository _repository;
        private readonly IClock _clock;

        public PurchaseCornHandler(
            ICornPurchaseRepository repository,
            IClock clock)
        {
            _repository = repository;
            _clock = clock;
        }

        public async Task<PurchaseResult> Handle(string clientId)
        {
            var now = _clock.UtcNow;

            var lastPurchase = await _repository.GetLastPurchaseAsync(clientId);
            var total = await _repository.GetTotalPurchasesAsync(clientId);

            if (lastPurchase.HasValue &&
                (now - lastPurchase.Value).TotalSeconds < 60)
            {
                var retryAt = lastPurchase.Value.AddMinutes(1);
                return PurchaseResult.Failure(retryAt);
            }

            await _repository.SetLastPurchaseAsync(clientId, now);

            return PurchaseResult.Success(now.AddMinutes(1));
        }
    }
}
