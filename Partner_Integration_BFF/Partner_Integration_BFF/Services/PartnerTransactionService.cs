using FluentValidation;
using Partner_Integration_BFF.Contracts.Models;
using Partner_Integration_BFF.Contracts.Requests;
using Partner_Integration_BFF.Messaging;

namespace Partner_Integration_BFF.Services;

public sealed class PartnerTransactionService(
    IValidator<PartnerTransactionRequest> validator,
    IMessagePublisher publisher,
    IVerificationPartner verificationPartner) : IPartnerTransactionService
{
    private readonly IMessagePublisher _publisher = publisher;
    private readonly IValidator<PartnerTransactionRequest> _validator = validator;
    private readonly IVerificationPartner _verificationPartner = verificationPartner;

    public async Task<PartnerTransactionOutcome> ProcessAsync(
        PartnerTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return PartnerTransactionOutcome.InvalidRequest;

        var partnerVerified = await _verificationPartner.VerifyAsync(
            request.PartnerId,
            cancellationToken);

        if (!partnerVerified)
            return PartnerTransactionOutcome.PartnerNotVerified;

        await _publisher.PublishAsync(request, cancellationToken);

        return PartnerTransactionOutcome.Accepted;
    }
}
