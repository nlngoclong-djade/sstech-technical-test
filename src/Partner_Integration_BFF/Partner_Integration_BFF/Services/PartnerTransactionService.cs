using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Partner_Integration_BFF.Contracts;
using Partner_Integration_BFF.Contracts.Models;
using Partner_Integration_BFF.Contracts.Responses;
using Partner_Integration_BFF.Messaging;
using Microsoft.Extensions.Http.Resilience;

namespace Partner_Integration_BFF.Services;

public class PartnerTransactionService(IValidator<PartnerTransactionRequest> validator,
    IMessagePublisher publisher,
    IVerificationPartner verificationPartner) : IPartnerTransactionService
{
    private readonly IMessagePublisher _publisher = publisher;
    private readonly IValidator<PartnerTransactionRequest> _validator = validator;
    private readonly IVerificationPartner _verificationPartner = verificationPartner;
    
    public async Task<bool> ProcessAsync(PartnerTransactionRequest request, CancellationToken cancellationToken)
    {
        /// validate partner request
        var requestValid = await _validator.ValidateAsync(request, cancellationToken);
        if (requestValid.IsValid)
        {
            /// verify partnerId
            if (await _verificationPartner.VerifyAsync(request.PartnerId, cancellationToken))
            {
                /// send message to message broker
                await _publisher.PublishAsync(
                    request,
                    cancellationToken);
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}