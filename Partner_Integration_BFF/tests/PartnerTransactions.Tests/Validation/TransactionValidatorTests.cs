using FluentValidation.TestHelper;
using Partner_Integration_BFF.Contracts;
using Partner_Integration_BFF.Contracts.Requests;
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

    [Fact(DisplayName = "Valid request passes because every required field satisfies its rule")]
    public void Validate_WhenRequestIsValid_ShouldHaveNoErrors()
    {
        var request = ValidRequest();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory(DisplayName = "Invalid amount fails because amount must be greater than zero")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-250)]
    public void Validate_WhenAmountIsNotGreaterThanZero_ShouldHaveError(decimal amount)
    {
        var request = ValidRequest() with { Amount = amount };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Theory(DisplayName = "Missing partner ID fails because partnerId is required")]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenPartnerIdIsMissing_ShouldHaveError(string? partnerId)
    {
        var request = ValidRequest() with { PartnerId = partnerId! };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PartnerId);
    }

    [Theory(DisplayName = "Missing transaction reference fails because transactionReference is required")]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenTransactionReferenceIsMissing_ShouldHaveError(string? reference)
    {
        var request = ValidRequest() with { TransactionReference = reference! };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TransactionReference);
    }

    [Theory(DisplayName = "Invalid currency fails because only USD, EUR, and VND are supported")]
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
