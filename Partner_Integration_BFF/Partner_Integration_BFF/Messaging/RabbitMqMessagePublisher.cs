using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Partner_Integration_BFF.Messaging;

public class RabbitMqMessagePublisher : IMessagePublisher
{
    private readonly ConnectionFactory _factory;
    private const string QueueName = "partner-transactions";

    public RabbitMqMessagePublisher(IConfiguration configuration)
    {
        _factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost"
        };
    }

    public async Task PublishAsync<T>(
        T message,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _factory.CreateConnectionAsync(cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: QueueName,
            body: body,
            cancellationToken: cancellationToken);
    }
}