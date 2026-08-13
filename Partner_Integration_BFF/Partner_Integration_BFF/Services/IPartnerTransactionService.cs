using Partner_Integration_BFF.Contracts.Models;
using Partner_Integration_BFF.Contracts.Requests;

namespace Partner_Integration_BFF.Services;

public interface IPartnerTransactionService
{
    Task<PartnerTransactionOutcome> ProcessAsync(
        PartnerTransactionRequest request,
        CancellationToken cancellationToken);
}
