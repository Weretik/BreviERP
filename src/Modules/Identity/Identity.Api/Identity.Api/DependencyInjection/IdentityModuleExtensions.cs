using BuildingBlocks.Application.DependencyInjection;
using Identity.Application;
using Identity.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Api.DependencyInjection;

public static class IdentityModuleExtensions
{
    /// <summary>Registers the Identity module: Application, Infrastructure, its Contracts implementations and controllers.</summary>
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityConfiguration(configuration);
        services.AddIdentityInfrastructure(configuration);
        services.AddModuleValidators(typeof(IdentityApplicationAssemblyMarker).Assembly);
        services.AddControllers().AddApplicationPart(typeof(IdentityApiAssemblyMarker).Assembly);
        return services;
    }
}
