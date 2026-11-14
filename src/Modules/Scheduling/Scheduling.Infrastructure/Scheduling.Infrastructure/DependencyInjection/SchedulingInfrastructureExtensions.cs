using BuildingBlocks.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scheduling.Application.Abstractions;
using Scheduling.Infrastructure.Integrations.CloudRun;
using Scheduling.Infrastructure.Integrations.Outbox;
using Scheduling.Infrastructure.Persistence;

namespace Scheduling.Infrastructure.DependencyInjection;

public static class SchedulingInfrastructureExtensions
{
    /// <summary>DbContext, migrator, the Cloud Run launcher and the tick's outbox work.</summary>
    public static IServiceCollection AddSchedulingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<SchedulingDbContext>(options => options.UseNpgsql(connection,
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", SchedulingDbContext.Schema)));
        services.AddScoped<ISchedulingDbContext>(sp => sp.GetRequiredService<SchedulingDbContext>());
        services.AddScoped<IDatabaseMigrator, DbMigrator<SchedulingDbContext>>();
        services.TryAddSingleton(TimeProvider.System);

        services.Configure<CloudRunJobOptions>(configuration.GetSection(CloudRunJobOptions.Section));
        services.AddHttpClient<ITaskJobLauncher, CloudRunTaskJobLauncher>(client => client.Timeout = TimeSpan.FromSeconds(30));
        services.AddScoped<ITickWork, OutboxTickWork>();
        return services;
    }
}
