using BobCorn.Application.Abstractions.Handlers;
using BobCorn.Application.Abstractions.Persistence;

namespace BobCorn.Application.UseCases.PurchaseCorn
{
    public class PurchaseCornHandler: IPurchaseCornHandler
    {
        private readonly ICornPurchaseRepository _repository;

        public PurchaseCornHandler(ICornPurchaseRepository repository)
        {
            _repository = repository;
        }

        public async Task<PurchaseResult> Handle(string clientId)
        {
            var total = await _repository.GetTotalPurchasesAsync(clientId);
            var (success, nextAllowedAt) = await _repository.TryPurchaseAsync(clientId);

            if (!success)
                return PurchaseResult.Failure(nextAllowedAt, total);

            return PurchaseResult.Success(nextAllowedAt, total);
        }
    }
}
