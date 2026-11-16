using ActivityLog.Api.Endpoints.Entries;
using ActivityLog.Application;
using ActivityLog.Infrastructure.DependencyInjection;
using BuildingBlocks.Application.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ActivityLog.Api.DependencyInjection;

public static class ActivityLogModuleExtensions
{
    /// <summary>Registers «Журнал дій» (ACT-001): Application validators and Infrastructure.</summary>
    public static IServiceCollection AddActivityLogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddActivityLogInfrastructure(configuration);
        services.AddModuleValidators(typeof(ActivityLogApplicationAssemblyMarker).Assembly);
        return services;
    }

    public static IEndpointRouteBuilder MapActivityLogEndpoints(this IEndpointRouteBuilder app) =>
        app.MapActivityEntriesEndpoints();
}
