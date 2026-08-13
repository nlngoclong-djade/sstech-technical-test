using System.Net;
using Microsoft.AspNetCore.Mvc;
using Partner_Integration_BFF.Contracts.Models;
using Partner_Integration_BFF.Contracts.Requests;
using Partner_Integration_BFF.Contracts.Responses;
using Partner_Integration_BFF.Services;

namespace Partner_Integration_BFF.Controllers;

[ApiController]
[Route("api/v1/partner/transactions")]
public sealed class PartnerIntegrationController(IPartnerTransactionService service) : ControllerBase
{
    private readonly IPartnerTransactionService _service = service;

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Post(PartnerTransactionRequest request, CancellationToken cancellationToken)
    {
        var outcome = await _service.ProcessAsync(request, cancellationToken);

        return outcome switch
        {
            PartnerTransactionOutcome.Accepted => Accepted(new ApiResponse<object?>(
                true,
                "Transaction request accepted for processing.",
                HttpStatusCode.Accepted,
                null)),
            PartnerTransactionOutcome.InvalidRequest => BadRequest(new ApiResponse<object?>(
                false,
                "Transaction request is invalid.",
                HttpStatusCode.BadRequest,
                null)),
            PartnerTransactionOutcome.PartnerNotVerified => UnprocessableEntity(new ApiResponse<object?>(
                false,
                "Partner could not be verified.",
                HttpStatusCode.UnprocessableEntity,
                null)),
            _ => throw new InvalidOperationException($"Unsupported transaction outcome: {outcome}.")
        };
    }
}
