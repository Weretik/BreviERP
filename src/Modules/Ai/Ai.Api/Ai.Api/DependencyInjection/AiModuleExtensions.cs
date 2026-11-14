using BuildingBlocks.Application.DependencyInjection;
using Ai.Application;
using Ai.Api.Endpoints.Settings;
using Ai.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ai.Api.DependencyInjection;

public static class AiModuleExtensions
{
    /// <summary>Registers the Ai module: Application, Infrastructure and its Contracts implementations.</summary>
    public static IServiceCollection AddAiModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAiInfrastructure(configuration);
        services.AddModuleValidators(typeof(AiApplicationAssemblyMarker).Assembly);
        return services;
    }

    /// <summary>Maps the Ai module's HTTP endpoints.</summary>
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder app) => app.MapAiSettingsEndpoints();
}
