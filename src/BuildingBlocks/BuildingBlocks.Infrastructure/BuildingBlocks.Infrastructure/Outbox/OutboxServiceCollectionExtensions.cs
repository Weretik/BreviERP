using BuildingBlocks.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Infrastructure.Outbox;

public static class OutboxServiceCollectionExtensions
{
    /// <summary>
    /// Adds the outbox of the module whose data lives in <typeparamref name="TContext"/> (its model calls
    /// <see cref="OutboxModelBuilderExtensions.AddOutbox"/>) and the shared dispatcher.
    /// </summary>
    public static IServiceCollection AddOutbox<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<IOutboxDispatcher, OutboxDispatcher>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOutboxModule, OutboxModule<TContext>>());
        return services;
    }

    public static IServiceCollection AddOutboxHandler<THandler>(this IServiceCollection services)
        where THandler : class, IOutboxHandler
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IOutboxHandler, THandler>());
        return services;
    }
}
