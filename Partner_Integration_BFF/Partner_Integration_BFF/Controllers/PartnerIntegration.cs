using System.Net;
using Microsoft.AspNetCore.Mvc;
using Partner_Integration_BFF.Contracts.Requests;
using Partner_Integration_BFF.Contracts.Responses;
using Partner_Integration_BFF.Services;

namespace Partner_Integration_BFF.Controllers;

[ApiController]
[Route("api/v1/partner/transactions")]
public class PartnerIntegrationController(IPartnerTransactionService service) : ControllerBase
{
    private readonly IPartnerTransactionService _service = service;

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Post(PartnerTransactionRequest request, CancellationToken cancellationToken)
    {
        await _service.ProcessAsync(request, cancellationToken);

        return Accepted(new ApiResponse<object?>(
            true,
            "Transaction request accepted for processing.",
            HttpStatusCode.Accepted,
            null));
    }
}
