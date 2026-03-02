using Accounting.Application;
using Crm.Application;
using FluentValidation;
using Reference.Application;

namespace Host.Api.DependencyInjection.ServiceCollections.HostServices;

public static class FluentValidationExtension
{
    public static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(AccountingApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);

        services.AddValidatorsFromAssembly(
            typeof(CrmApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);

        services.AddValidatorsFromAssembly(
            typeof(ReferenceApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);


        return services;
    }
}

