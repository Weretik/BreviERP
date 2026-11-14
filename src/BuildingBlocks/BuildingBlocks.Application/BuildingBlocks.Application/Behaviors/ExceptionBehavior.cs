using BuildingBlocks.Application.Logging;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BuildingBlocks.Application.Behaviors;

public sealed class ExceptionBehavior<TMessage, TResponse>(
    ILogger<ExceptionBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(next);
            return await next(message, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            var messageType = typeof(TMessage).Name;
            CancellationLog.Cancelled(logger, messageType);

            throw;
        }
        catch (Exception ex) when (IsCritical(ex))
        {
            var messageType = typeof(TMessage).Name;
            var exType = ex.GetType().Name;

            UnhandledLog.Critical(logger, messageType, exType, ex);
            throw;
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return CreateConflictResult();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresException &&
                                           postgresException.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return CreateConflictResult();
        }
    }

    private static bool IsCritical(Exception ex)
        => ex is OutOfMemoryException or AccessViolationException or ThreadAbortException;

    private static TResponse CreateConflictResult()
    {
        const string errorMessage = "Запис із такими унікальними даними вже існує.";

        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Conflict(errorMessage);

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var result = typeof(Result<>)
                .MakeGenericType(typeof(TResponse).GetGenericArguments()[0])
                .GetMethods()
                .Single(method => method.Name == nameof(Result<object>.Conflict) && method.GetParameters().Length == 1)
                .Invoke(null, [new[] { errorMessage }]);
            return (TResponse)result!;
        }

        throw new InvalidOperationException($"Cannot map a conflict to {typeof(TResponse).Name}.");
    }
}
