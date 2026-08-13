namespace Partner_Integration_BFF.Services;

public interface IVerificationPartner
{
    Task<bool> VerifyAsync(string partnerId, CancellationToken cancellationToken);
}