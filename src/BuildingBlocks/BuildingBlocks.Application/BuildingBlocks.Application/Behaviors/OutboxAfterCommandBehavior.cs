using BuildingBlocks.Application.Outbox;

namespace BuildingBlocks.Application.Behaviors;

/// <summary>
/// After a use case, tries once the outbox messages its saves wrote from domain events (ADR-0016); the scheduler tick
/// retries whatever fails. The dispatcher never throws, so the use case's answer stays as it was.
/// </summary>
public sealed class OutboxAfterCommandBehavior<TMessage, TResponse>(ISavedOutboxMessages saved, IOutboxDispatcher outbox)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);
        var response = await next(message, cancellationToken);
        var ids = saved.Take();
        if (ids.Count > 0) await outbox.DispatchAsync(ids, CancellationToken.None);
        return response;
    }
}
