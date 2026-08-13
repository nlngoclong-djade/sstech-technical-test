using Partner_Integration_BFF.Contracts.Models;
using Partner_Integration_BFF.Contracts.Responses;

namespace Partner_Integration_BFF.Services;

public class VerificationPartner(HttpClient httpClient) : IVerificationPartner
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<bool> VerifyAsync(
        string partnerId,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(
            $"api/mock/partners/{partnerId}",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<
                ApiResponse<VerifyPartnerResult>>(
                cancellationToken);

        return result?.IsSuccess == true;
    }
}