using MediatR;

namespace PaymentService.Tests.Fakes;

internal sealed class TestMediator : IMediator
{
    private readonly Func<object, CancellationToken, Task<object?>> _handler;

    public TestMediator(Func<object, CancellationToken, Task<object?>> handler)
    {
        _handler = handler;
    }

    public object? LastRequest { get; private set; }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        return (TResponse)(await _handler(request, cancellationToken))!;
    }

    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        LastRequest = request;
        await _handler(request!, cancellationToken);
    }

    public async Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        return await _handler(request, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default) =>
        Empty<TResponse>();

    public IAsyncEnumerable<object?> CreateStream(
        object request,
        CancellationToken cancellationToken = default) =>
        Empty<object?>();

    public Task Publish(object notification, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification =>
        Task.CompletedTask;

    private static async IAsyncEnumerable<T> Empty<T>()
    {
        await Task.CompletedTask;
        yield break;
    }
}
