using BuildingBlocks.Application.DependencyInjection;
using Accounting.Application;
using Accounting.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Accounting.Api.DependencyInjection;

public static class AccountingModuleExtensions
{
    /// <summary>Registers the Accounting module: Application, Infrastructure, its Contracts implementations and controllers.</summary>
    public static IServiceCollection AddAccountingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAccountingInfrastructure(configuration);
        services.AddModuleValidators(typeof(AccountingApplicationAssemblyMarker).Assembly);
        services.AddControllers().AddApplicationPart(typeof(AccountingApiAssemblyMarker).Assembly);
        return services;
    }
}
