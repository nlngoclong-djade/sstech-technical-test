using System.Net;
using Microsoft.AspNetCore.Mvc;
using Partner_Integration_BFF.Contracts.Models;
using Partner_Integration_BFF.Contracts.Responses;

namespace Partner_Integration_BFF.Controllers;

[ApiController]
[Route("api/mock/partners")]
public class MockPartnersController() : ControllerBase
{
    private readonly Random _random = new();
   
    [HttpGet("{partnerId}")]
    public IActionResult Get(string partnerId)
    {
        if (_random.NextDouble() < 0.3)
        {
            return StatusCode(
                StatusCodes.Status408RequestTimeout,
                new ApiResponse<VerifyPartnerResult>(
                    false,
                    "Partner verification timed out.",
                    HttpStatusCode.RequestTimeout,
                    new VerifyPartnerResult(partnerId, false)
                ));
        }

        return Ok(new ApiResponse<VerifyPartnerResult>(
            true,
            "Verification Successful",
            HttpStatusCode.OK,
            new VerifyPartnerResult(
                partnerId,
                true)
            )
        );
    }
}
