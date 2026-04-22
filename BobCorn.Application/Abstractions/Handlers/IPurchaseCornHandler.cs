using BobCorn.Application.UseCases.PurchaseCorn;

namespace BobCorn.Application.Abstractions.Handlers
{
    public interface IPurchaseCornHandler
    {
        Task<PurchaseResult> Handle(string clientId);
    }
}
