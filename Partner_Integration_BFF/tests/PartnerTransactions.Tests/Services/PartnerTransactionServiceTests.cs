using Moq;
using Partner_Integration_BFF.Contracts.Models;
using Partner_Integration_BFF.Contracts.Requests;
using Partner_Integration_BFF.Messaging;
using Partner_Integration_BFF.Services;
using Partner_Integration_BFF.Validators;
using Xunit;

namespace PartnerTransactions.Tests.Services;

public class PartnerTransactionServiceTests
{
    private static PartnerTransactionRequest ValidRequest() =>
        new(
            PartnerId: "P-1001",
            TransactionReference: "TXN-99823",
            Amount: 250.00m,
            Currency: "USD",
            Timestamp: DateTimeOffset.Parse("2024-05-10T14:30:00Z"));

    [Fact(DisplayName = "Invalid transaction stops before partner verification and publishing")]
    public async Task ProcessAsync_WhenRequestIsInvalid_ShouldStopProcessing()
    {
        var request = ValidRequest() with { Amount = 0 };
        var verificationPartner = new Mock<IVerificationPartner>();
        var publisher = new Mock<IMessagePublisher>();
        var service = new PartnerTransactionService(
            new TransactionValidator(),
            publisher.Object,
            verificationPartner.Object);

        var outcome = await service.ProcessAsync(request, CancellationToken.None);

        Assert.Equal(PartnerTransactionOutcome.InvalidRequest, outcome);
        verificationPartner.Verify(
            x => x.VerifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        publisher.Verify(
            x => x.PublishAsync(
                It.IsAny<PartnerTransactionRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Unverified partner stops before publishing its transaction")]
    public async Task ProcessAsync_WhenPartnerIsNotVerified_ShouldNotPublish()
    {
        var request = ValidRequest();
        var verificationPartner = new Mock<IVerificationPartner>();
        verificationPartner
            .Setup(x => x.VerifyAsync(request.PartnerId, CancellationToken.None))
            .ReturnsAsync(false);
        var publisher = new Mock<IMessagePublisher>();
        var service = new PartnerTransactionService(
            new TransactionValidator(),
            publisher.Object,
            verificationPartner.Object);

        var outcome = await service.ProcessAsync(request, CancellationToken.None);

        Assert.Equal(PartnerTransactionOutcome.PartnerNotVerified, outcome);
        verificationPartner.Verify(
            x => x.VerifyAsync(request.PartnerId, CancellationToken.None),
            Times.Once);
        publisher.Verify(
            x => x.PublishAsync(
                It.IsAny<PartnerTransactionRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Verified valid transaction is published once before it is accepted")]
    public async Task ProcessAsync_WhenRequestAndPartnerAreValid_ShouldPublishAndAccept()
    {
        var request = ValidRequest();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var verificationPartner = new Mock<IVerificationPartner>();
        verificationPartner
            .Setup(x => x.VerifyAsync(request.PartnerId, cancellationToken))
            .ReturnsAsync(true);
        var publisher = new Mock<IMessagePublisher>();
        publisher
            .Setup(x => x.PublishAsync(request, cancellationToken))
            .Returns(Task.CompletedTask);
        var service = new PartnerTransactionService(
            new TransactionValidator(),
            publisher.Object,
            verificationPartner.Object);

        var outcome = await service.ProcessAsync(request, cancellationToken);

        Assert.Equal(PartnerTransactionOutcome.Accepted, outcome);
        verificationPartner.Verify(
            x => x.VerifyAsync(request.PartnerId, cancellationToken),
            Times.Once);
        publisher.Verify(
            x => x.PublishAsync(request, cancellationToken),
            Times.Once);
    }
}
