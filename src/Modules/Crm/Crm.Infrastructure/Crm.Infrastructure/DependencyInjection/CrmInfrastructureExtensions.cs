using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Infrastructure.DependencyInjection;

public static class CrmInfrastructureExtensions
{
    public static IServiceCollection AddCrmInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCrmDbContextServices(configuration);
        services.AddCrmServices(configuration);

        return services;
    }
}
