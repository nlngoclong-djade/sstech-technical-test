namespace Partner_Integration_BFF.Contracts;

public sealed record PartnerTransactionRequest(
    string PartnerId, 
    string TransactionReference, 
    decimal Amount, 
    string Currency, 
    string Timestamp
);