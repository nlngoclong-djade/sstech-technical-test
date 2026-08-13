namespace Partner_Integration_BFF.Contracts.Models;

public sealed record VerifyPartnerResult(
    string PartnerId,
    bool Valid);