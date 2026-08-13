using System.Net;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Partner_Integration_BFF.Contracts.Models;
using Partner_Integration_BFF.Contracts.Requests;
using Partner_Integration_BFF.Contracts.Responses;
using Partner_Integration_BFF.Controllers;
using Partner_Integration_BFF.Services;
using Xunit;

namespace PartnerTransactions.Tests.Controllers;

public class PartnerIntegrationControllerTests
{
    private static PartnerTransactionRequest ValidRequest() =>
        new(
            PartnerId: "P-1001",
            TransactionReference: "TXN-99823",
            Amount: 250.00m,
            Currency: "USD",
            Timestamp: DateTimeOffset.Parse("2024-05-10T14:30:00Z"));

    [Fact(DisplayName = "Accepted transaction includes a message so the caller knows it will be processed")]
    public async Task Post_WhenRequestIsAccepted_ShouldReturnProcessingMessage()
    {
        var request = ValidRequest();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var service = new Mock<IPartnerTransactionService>();
        service
            .Setup(x => x.ProcessAsync(request, cancellationToken))
            .ReturnsAsync(PartnerTransactionOutcome.Accepted);
        var controller = new PartnerIntegrationController(service.Object);

        var result = await controller.Post(request, cancellationToken);

        var accepted = Assert.IsType<AcceptedResult>(result);
        Assert.Equal((int)HttpStatusCode.Accepted, accepted.StatusCode);

        var response = Assert.IsType<ApiResponse<object?>>(accepted.Value);
        Assert.True(response.IsSuccess);
        Assert.Equal("Transaction request accepted for processing.", response.Message);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Null(response.Data);

        service.Verify(
            x => x.ProcessAsync(request, cancellationToken),
            Times.Once);
    }

    [Fact(DisplayName = "Invalid transaction returns 400 because it was not accepted for processing")]
    public async Task Post_WhenRequestIsInvalid_ShouldReturnBadRequest()
    {
        var request = ValidRequest() with { Amount = 0 };
        var service = new Mock<IPartnerTransactionService>();
        service
            .Setup(x => x.ProcessAsync(request, CancellationToken.None))
            .ReturnsAsync(PartnerTransactionOutcome.InvalidRequest);
        var controller = new PartnerIntegrationController(service.Object);

        var result = await controller.Post(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object?>>(badRequest.Value);
        Assert.False(response.IsSuccess);
        Assert.Equal("Transaction request is invalid.", response.Message);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Data);

        service.Verify(
            x => x.ProcessAsync(request, CancellationToken.None),
            Times.Once);
    }

    [Fact(DisplayName = "Unverified partner returns 422 because its transaction cannot be processed")]
    public async Task Post_WhenPartnerIsNotVerified_ShouldReturnUnprocessableEntity()
    {
        var request = ValidRequest();
        var service = new Mock<IPartnerTransactionService>();
        service
            .Setup(x => x.ProcessAsync(request, CancellationToken.None))
            .ReturnsAsync(PartnerTransactionOutcome.PartnerNotVerified);
        var controller = new PartnerIntegrationController(service.Object);

        var result = await controller.Post(request, CancellationToken.None);

        var unprocessable = Assert.IsType<UnprocessableEntityObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object?>>(unprocessable.Value);
        Assert.False(response.IsSuccess);
        Assert.Equal("Partner could not be verified.", response.Message);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Null(response.Data);

        service.Verify(
            x => x.ProcessAsync(request, CancellationToken.None),
            Times.Once);
    }
}
