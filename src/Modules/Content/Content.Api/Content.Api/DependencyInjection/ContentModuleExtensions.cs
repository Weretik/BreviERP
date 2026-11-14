using BuildingBlocks.Application.DependencyInjection;
using Content.Application;
using Content.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Content.Api.DependencyInjection;

public static class ContentModuleExtensions
{
    /// <summary>Registers the Content module: Application, Infrastructure, its Contracts implementations and controllers.</summary>
    public static IServiceCollection AddContentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddContentInfrastructure(configuration);
        services.AddModuleValidators(typeof(ContentApplicationAssemblyMarker).Assembly);
        services.AddControllers().AddApplicationPart(typeof(ContentApiAssemblyMarker).Assembly);
        return services;
    }
}
