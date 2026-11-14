using BuildingBlocks.Application.DependencyInjection;
using Crm.Application;
using Crm.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Api.DependencyInjection;

public static class CrmModuleExtensions
{
    /// <summary>Registers the Crm module: Application, Infrastructure, its Contracts implementations and controllers.</summary>
    public static IServiceCollection AddCrmModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCrmDbContextServices(configuration);
        services.AddCrmKeepinCrmSync(configuration);
        services.AddModuleValidators(typeof(CrmApplicationAssemblyMarker).Assembly);
        services.AddControllers().AddApplicationPart(typeof(CrmApiAssemblyMarker).Assembly);
        return services;
    }
}
