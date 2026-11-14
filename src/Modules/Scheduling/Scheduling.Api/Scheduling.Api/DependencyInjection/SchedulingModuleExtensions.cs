using BuildingBlocks.Application.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scheduling.Api.Endpoints.Tasks;
using Scheduling.Api.Endpoints.Tick;
using Scheduling.Api.Endpoints.Tick.RunSchedulerTick;
using Scheduling.Application;
using Scheduling.Infrastructure.DependencyInjection;

namespace Scheduling.Api.DependencyInjection;

public static class SchedulingModuleExtensions
{
    /// <summary>Registers the Scheduling module: Application validators, Infrastructure and the tick's settings.</summary>
    public static IServiceCollection AddSchedulingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSchedulingInfrastructure(configuration);
        services.AddModuleValidators(typeof(SchedulingApplicationAssemblyMarker).Assembly);
        services.Configure<SchedulerTickOptions>(configuration.GetSection(SchedulerTickOptions.Section));
        return services;
    }

    /// <summary>Maps «Фонові задачі» and the scheduler tick.</summary>
    public static IEndpointRouteBuilder MapSchedulingEndpoints(this IEndpointRouteBuilder app) =>
        app.MapScheduledTasksEndpoints().MapSchedulerTick();
}
