using BuildingBlocks.Application.Actors;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Infrastructure.DomainEvents;

public static class DomainEventRegistrationExtensions
{
    /// <summary>
    /// The shared parts of domain events: the save interceptor, the saved-ids list of the scope, the outbox dispatcher
    /// that <see cref="BuildingBlocks.Application.Behaviors.OutboxAfterCommandBehavior{TMessage,TResponse}"/> uses and,
    /// unless the host registers a real one, an actor that is nobody.
    /// </summary>
    public static IServiceCollection AddDomainEvents(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<ICurrentActor, NoCurrentActor>();
        services.TryAddScoped<ISavedOutboxMessages, SavedOutboxMessages>();
        services.TryAddScoped<IDomainEventSuppression, DomainEventSuppression>();
        services.TryAddScoped<DomainEventOutboxInterceptor>();
        services.TryAddScoped<IOutboxDispatcher, OutboxDispatcher>();
        return services;
    }

    /// <summary>Lets the module's saves write its domain events to its outbox (the context must map the outbox).</summary>
    public static DbContextOptionsBuilder AddDomainEvents(this DbContextOptionsBuilder options, IServiceProvider provider) =>
        options.AddInterceptors(provider.GetRequiredService<DomainEventOutboxInterceptor>());
}
