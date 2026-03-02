using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Infrastructure.DependencyInjection;

public static class CrmServicesExtension
{
    public static IServiceCollection AddCrmServices(
        this IServiceCollection services, IConfiguration configuration)
    {


        return services;
    }
}
