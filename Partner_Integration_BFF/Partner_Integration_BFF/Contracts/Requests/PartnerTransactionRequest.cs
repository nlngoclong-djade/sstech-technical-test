namespace Partner_Integration_BFF.Contracts.Requests;

public sealed record PartnerTransactionRequest(
    string PartnerId, 
    string TransactionReference, 
    decimal Amount, 
    string Currency, 
    DateTimeOffset Timestamp
);