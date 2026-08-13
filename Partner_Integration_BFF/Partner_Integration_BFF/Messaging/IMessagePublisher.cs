namespace Partner_Integration_BFF.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(
        T message,
        CancellationToken cancellationToken = default);
}