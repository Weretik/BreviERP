using BuildingBlocks.Application.DependencyInjection;
using Ai.Application;
using Ai.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ai.Api.DependencyInjection;

public static class AiModuleExtensions
{
    /// <summary>Registers the Ai module: Application, Infrastructure, its Contracts implementations and controllers.</summary>
    public static IServiceCollection AddAiModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAiInfrastructure(configuration);
        services.AddModuleValidators(typeof(AiApplicationAssemblyMarker).Assembly);
        services.AddControllers().AddApplicationPart(typeof(AiApiAssemblyMarker).Assembly);
        return services;
    }
}
