using BuildingBlocks.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Outbox;

/// <summary>The outbox of one module: its <typeparamref name="TContext"/> and its <c>OutboxMessages</c> table.</summary>
internal interface IOutboxModule
{
    Task<OutboxRun> RunAsync(IReadOnlyCollection<Guid>? ids, CancellationToken cancellationToken);
}

internal sealed class OutboxModule<TContext>(
    IServiceScopeFactory scopes, TimeProvider clock, ILogger<OutboxModule<TContext>> logger) : IOutboxModule
    where TContext : DbContext
{
    public const int BatchSize = 20;

    /// <summary>
    /// Claims due rows with <c>FOR UPDATE SKIP LOCKED</c>, so a scheduled run and a request's own try never take the
    /// same row; runs each handler in its own scope and saves the result in the claiming transaction.
    /// </summary>
    public async Task<OutboxRun> RunAsync(IReadOnlyCollection<Guid>? ids, CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        var schema = db.Model.GetDefaultSchema() ?? "public";
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var now = clock.GetUtcNow();
        var filter = ids is null ? "" : """ AND "Id" = ANY({1})""";
        var messages = await db.Set<OutboxMessage>()
            .FromSqlRaw(
                $$"""
                  SELECT * FROM "{{schema}}"."{{OutboxModelBuilderExtensions.Table}}"
                  WHERE "ProcessedAt" IS NULL AND "Attempts" < {{OutboxMessage.MaxAttempts}} AND "NextAttemptAt" <= {0}{{filter}}
                  ORDER BY "CreatedAt" LIMIT {{BatchSize}} FOR UPDATE SKIP LOCKED
                  """,
                ids is null ? [now] : [now, ids.ToArray()])
            .ToListAsync(cancellationToken);
        if (messages.Count == 0) return OutboxRun.Empty;

        var run = OutboxRun.Empty;
        foreach (var message in messages)
        {
            var error = await HandleAsync(message, cancellationToken);
            var at = clock.GetUtcNow();
            if (error is null)
            {
                message.Succeed(at);
                run = run.Add(new OutboxRun(1, 0));
                continue;
            }

            message.Fail(error, at);
            run = run.Add(new OutboxRun(0, 1));
            if (message.GaveUp)
                logger.LogError("Outbox message {Type} {Id} failed {Attempts} times: {Error}", message.Type, message.Id,
                    message.Attempts, message.LastError);
            else
                logger.LogWarning("Outbox message {Type} {Id} failed, next try at {NextAttemptAt}: {Error}",
                    message.Type, message.Id, message.NextAttemptAt, message.LastError);
        }

        await db.SaveChangesAsync(CancellationToken.None);
        await transaction.CommitAsync(CancellationToken.None);
        return run;
    }

    private async Task<string?> HandleAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetServices<IOutboxHandler>().FirstOrDefault(x => x.Type == message.Type);
        if (handler is null) return $"No handler for {message.Type}.";
        try
        {
            return await handler.HandleAsync(message.Payload, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException
                                              || !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Outbox handler {Type} threw for {Id}", message.Type, message.Id);
            return exception.Message;
        }
    }
}
