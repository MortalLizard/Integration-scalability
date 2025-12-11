namespace Shared;

public interface IConsumer : IAsyncDisposable
{
    public Task StartAsync(string queueName, Func<string, CancellationToken, Task> handler, CancellationToken cancellationToken = default);
}
    
