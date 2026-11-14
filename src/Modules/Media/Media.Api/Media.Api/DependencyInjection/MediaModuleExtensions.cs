using BuildingBlocks.Application.DependencyInjection;
using Media.Application;
using Media.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Media.Api.DependencyInjection;

public static class MediaModuleExtensions
{
    /// <summary>Registers the Media module: Application, Infrastructure, its Contracts implementations and controllers.</summary>
    public static IServiceCollection AddMediaModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediaInfrastructure(configuration);
        services.AddModuleValidators(typeof(MediaApplicationAssemblyMarker).Assembly);
        services.AddControllers().AddApplicationPart(typeof(MediaApiAssemblyMarker).Assembly);
        return services;
    }
}
