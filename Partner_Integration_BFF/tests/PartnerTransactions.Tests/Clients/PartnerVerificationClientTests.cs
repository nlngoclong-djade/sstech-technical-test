using Partner_Integration_BFF.Services;
using PartnerTransactions.Tests.TestHelpers;
using Xunit;

namespace PartnerTransactions.Tests.Clients;

public class PartnerVerificationClientTests
{
    [Fact(DisplayName = "Valid partner response returns true because isSuccess is true")]
    public async Task VerifyAsync_WhenApiReturnsValidPartner_ShouldReturnTrue()
    {
        const string json = """
        {
          "isSuccess": true,
          "message": "Verification Successful",
          "statusCode": 200,
          "data": {
            "partnerId": "P-1001",
            "valid": true
          }
        }
        """;

        var handler = new SequenceHttpMessageHandler(
            _ => SequenceHttpMessageHandler.Ok(json));

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var client = new VerificationPartner(httpClient);

        var result = await client.VerifyAsync("P-1001", CancellationToken.None);

        Assert.True(result, "Expected true because the response contains isSuccess: true.");
        Assert.Equal(1, handler.CallCount);
    }

    [Theory(DisplayName = "Partner verification rejects inconsistent or unsuccessful responses")]
    [InlineData("""{"isSuccess":false,"message":"Rejected","statusCode":200,"data":{"partnerId":"P-1001","valid":true}}""")]
    [InlineData("""{"isSuccess":true,"message":"Rejected","statusCode":200,"data":{"partnerId":"P-1001","valid":false}}""")]
    [InlineData("""{"isSuccess":true,"message":"Missing data","statusCode":200,"data":null}""")]
    [InlineData("""{"isSuccess":true,"message":"Wrong partner","statusCode":200,"data":{"partnerId":"P-9999","valid":true}}""")]
    public async Task VerifyAsync_WhenResponseDoesNotConfirmRequestedPartner_ShouldReturnFalse(
        string json)
    {
        var handler = new SequenceHttpMessageHandler(
            request =>
            {
                Assert.Equal("/api/mock/partners/P-1001", request.RequestUri?.AbsolutePath);
                return SequenceHttpMessageHandler.Ok(json);
            });
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };
        var client = new VerificationPartner(httpClient);

        var result = await client.VerifyAsync("P-1001", CancellationToken.None);

        Assert.False(result);
        Assert.Equal(1, handler.CallCount);
    }
}
