using Microsoft.AspNetCore.Mvc;
using Partner_Integration_BFF.Contracts;
using Partner_Integration_BFF.Contracts.Requests;
using Partner_Integration_BFF.Services;

namespace Partner_Integration_BFF.Controllers;

[ApiController]
[Route("api/v1/partner/transactions")]
public class PartnerIntegrationController(IPartnerTransactionService service) : ControllerBase
{
    private readonly IPartnerTransactionService _service = service;

    [HttpPost]
    public async Task<IActionResult> Post(PartnerTransactionRequest request, CancellationToken cancellationToken)
    {
        await _service.ProcessAsync(request, cancellationToken);

        return Accepted();
    }
}