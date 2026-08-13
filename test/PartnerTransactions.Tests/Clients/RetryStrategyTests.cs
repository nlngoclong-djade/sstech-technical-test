using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using PartnerTransactions.Tests.TestHelpers;
using Polly;
using Xunit;

namespace PartnerTransactions.Tests.Clients;

public class RetryStrategyTests
{
    [Fact]
    public async Task RetryPipeline_WhenFirstTwoAttemptsTimeout_ShouldRetryAndSucceed()
    {
        var handler = new SequenceHttpMessageHandler(
            _ => SequenceHttpMessageHandler.RequestTimeout(),
            _ => SequenceHttpMessageHandler.RequestTimeout(),
            _ => SequenceHttpMessageHandler.Ok("""{"success":true}"""));

        var services = new ServiceCollection();

        services
            .AddHttpClient("partner-verification", client =>
            {
                client.BaseAddress = new Uri("http://localhost");
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddResilienceHandler("retry-test", builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.Zero,
                    BackoffType = DelayBackoffType.Constant,
                    UseJitter = false
                });
            });

        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("partner-verification");

        var response = await client.GetAsync("/api/mock/partners/P-1001");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, handler.CallCount);
    }

    [Fact]
    public async Task RetryPipeline_WhenAllAttemptsTimeout_ShouldReturnFinalFailure()
    {
        var handler = new SequenceHttpMessageHandler(
            _ => SequenceHttpMessageHandler.RequestTimeout(),
            _ => SequenceHttpMessageHandler.RequestTimeout(),
            _ => SequenceHttpMessageHandler.RequestTimeout(),
            _ => SequenceHttpMessageHandler.RequestTimeout());

        var services = new ServiceCollection();

        services
            .AddHttpClient("partner-verification", client =>
            {
                client.BaseAddress = new Uri("http://localhost");
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddResilienceHandler("retry-test", builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.Zero,
                    BackoffType = DelayBackoffType.Constant,
                    UseJitter = false
                });
            });

        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("partner-verification");

        var response = await client.GetAsync("/api/mock/partners/P-1001");

        Assert.Equal(HttpStatusCode.RequestTimeout, response.StatusCode);

        // Initial request + 3 retries.
        Assert.Equal(4, handler.CallCount);
    }
}
