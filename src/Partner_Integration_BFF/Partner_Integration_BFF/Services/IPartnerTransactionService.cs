using Partner_Integration_BFF.Contracts;

namespace Partner_Integration_BFF.Services;

public interface IPartnerTransactionService
{
    Task<bool> ProcessAsync(PartnerTransactionRequest request, CancellationToken cancellationToken);
}