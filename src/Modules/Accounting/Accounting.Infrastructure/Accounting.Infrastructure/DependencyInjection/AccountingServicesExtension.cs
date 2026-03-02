using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Accounting.Infrastructure.DependencyInjection;

public static class AccountingServicesExtension
{
    public static IServiceCollection AddAccountingServices(
        this IServiceCollection services, IConfiguration configuration)
    {


        return services;
    }
}
