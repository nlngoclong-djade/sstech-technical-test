using Partner_Integration_BFF.Contracts.Models;
using Partner_Integration_BFF.Contracts.Responses;

namespace Partner_Integration_BFF.Services;

public sealed class VerificationPartner(HttpClient httpClient) : IVerificationPartner
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<bool> VerifyAsync(
        string partnerId,
        CancellationToken cancellationToken)
    {
        var encodedPartnerId = Uri.EscapeDataString(partnerId);
        var response = await _httpClient.GetAsync(
            $"api/mock/partners/{encodedPartnerId}",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<VerifyPartnerResult>>(
            cancellationToken);

        var verifiedPartner = result?.Data;

        return result?.IsSuccess == true &&
               verifiedPartner?.Valid == true &&
               string.Equals(
                   verifiedPartner.PartnerId,
                   partnerId,
                   StringComparison.Ordinal);
    }
}
