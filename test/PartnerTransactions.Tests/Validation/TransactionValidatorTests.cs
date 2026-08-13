using FluentValidation.TestHelper;
using Partner_Integration_BFF.Contracts;
using Partner_Integration_BFF.Validators;
using Xunit;

namespace PartnerTransactions.Tests.Validation;

public class TransactionValidatorTests
{
    private readonly TransactionValidator _validator = new();

    private static PartnerTransactionRequest ValidRequest() =>
        new(
            PartnerId: "P-1001",
            TransactionReference: "TXN-99823",
            Amount: 250.00m,
            Currency: "USD",
            Timestamp: DateTimeOffset.Parse("2024-05-10T14:30:00Z"));

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldHaveNoErrors()
    {
        var request = ValidRequest();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-250)]
    public void Validate_WhenAmountIsNotGreaterThanZero_ShouldHaveError(decimal amount)
    {
        var request = ValidRequest() with { Amount = amount };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenPartnerIdIsMissing_ShouldHaveError(string? partnerId)
    {
        var request = ValidRequest() with { PartnerId = partnerId! };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PartnerId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenTransactionReferenceIsMissing_ShouldHaveError(string? reference)
    {
        var request = ValidRequest() with { TransactionReference = reference! };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TransactionReference);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ABC")]
    [InlineData("US")]
    public void Validate_WhenCurrencyIsInvalid_ShouldHaveError(string currency)
    {
        var request = ValidRequest() with { Currency = currency };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }
}
