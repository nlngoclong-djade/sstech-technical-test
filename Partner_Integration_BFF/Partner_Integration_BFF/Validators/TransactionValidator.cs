using FluentValidation;
using Partner_Integration_BFF.Contracts.Requests;

namespace Partner_Integration_BFF.Validators;

public sealed class TransactionValidator : AbstractValidator<PartnerTransactionRequest>
{
    private static readonly HashSet<string> SupportedCurrencies =
    [
        "USD",
        "EUR",
        "VND"
    ];

    public TransactionValidator()
    {
        RuleFor(x => x.PartnerId)
            .NotEmpty();

        RuleFor(x => x.TransactionReference)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Must(currency => SupportedCurrencies.Contains(currency))
            .WithMessage("Unsupported currency.");

        RuleFor(x => x.Timestamp)
            .NotEmpty();
    }
}
