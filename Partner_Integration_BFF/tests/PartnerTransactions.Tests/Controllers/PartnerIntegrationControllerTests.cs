using System.Net;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Partner_Integration_BFF.Contracts.Requests;
using Partner_Integration_BFF.Contracts.Responses;
using Partner_Integration_BFF.Controllers;
using Partner_Integration_BFF.Services;
using Xunit;

namespace PartnerTransactions.Tests.Controllers;

public class PartnerIntegrationControllerTests
{
    [Fact(DisplayName = "Accepted transaction includes a message so the caller knows it will be processed")]
    public async Task Post_WhenRequestIsAccepted_ShouldReturnProcessingMessage()
    {
        var request = new PartnerTransactionRequest(
            PartnerId: "P-1001",
            TransactionReference: "TXN-99823",
            Amount: 250.00m,
            Currency: "USD",
            Timestamp: DateTimeOffset.Parse("2024-05-10T14:30:00Z"));
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var service = new Mock<IPartnerTransactionService>();
        service
            .Setup(x => x.ProcessAsync(request, cancellationToken))
            .ReturnsAsync(true);
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
}
