using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Accounting.Infrastructure.DependencyInjection;

public static class AccountingInfrastructureExtensions
{
    public static IServiceCollection AddAccountingInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAccountingDbContextServices(configuration);
        services.AddAccountingServices(configuration);

        return services;
    }
}
