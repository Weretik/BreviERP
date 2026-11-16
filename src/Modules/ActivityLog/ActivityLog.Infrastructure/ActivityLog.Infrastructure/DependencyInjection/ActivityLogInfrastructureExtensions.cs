using ActivityLog.Application.Abstractions;
using ActivityLog.Application.Features.Entries.EventHandlers;
using ActivityLog.Infrastructure.Adapters.Identity;
using ActivityLog.Infrastructure.Persistence;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Infrastructure.Database;
using BuildingBlocks.Infrastructure.DomainEvents;
using BuildingBlocks.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ActivityLog.Infrastructure.DependencyInjection;

public static class ActivityLogInfrastructureExtensions
{
    /// <summary>
    /// DbContext, migrator, the translation of every module's domain events into <c>activity.recorded</c>, its handler
    /// and the shared domain-event parts.
    /// </summary>
    public static IServiceCollection AddActivityLogInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default")
                         ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<ActivityLogDbContext>(options => options.UseNpgsql(connection,
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", ActivityLogDbContext.Schema)));
        services.AddScoped<IActivityLogDbContext>(sp => sp.GetRequiredService<ActivityLogDbContext>());
        services.AddScoped<IDatabaseMigrator, DbMigrator<ActivityLogDbContext>>();
        services.TryAddSingleton(TimeProvider.System);

        services.AddDomainEvents();
        services.AddOutboxHandler<RecordActivityOutboxHandler>();
        services.AddScoped<IDomainEventOutbox, ActivityDomainEventOutbox>();
        services.AddScoped<IActorNames, StaffActorNames>();
        return services;
    }
}
