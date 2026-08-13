using System.Net;

namespace PartnerTransactions.Tests.TestHelpers;

public sealed class SequenceHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses;

    public int CallCount { get; private set; }

    public SequenceHttpMessageHandler(
        params Func<HttpRequestMessage, HttpResponseMessage>[] responses)
    {
        _responses = new Queue<Func<HttpRequestMessage, HttpResponseMessage>>(responses);
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        CallCount++;

        if (_responses.Count == 0)
            throw new InvalidOperationException("No configured HTTP response remains.");

        var response = _responses.Dequeue()(request);
        return Task.FromResult(response);
    }

    public static HttpResponseMessage RequestTimeout() =>
        new(HttpStatusCode.RequestTimeout);

    public static HttpResponseMessage Ok(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(
                json,
                System.Text.Encoding.UTF8,
                "application/json")
        };
}
