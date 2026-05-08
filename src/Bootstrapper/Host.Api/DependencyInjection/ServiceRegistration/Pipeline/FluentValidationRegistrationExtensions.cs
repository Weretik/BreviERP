using Accounting.Application;
using Crm.Application;
using FluentValidation;
using Identity.Application;
using Reference.Application;

namespace Host.Api.DependencyInjection.ServiceRegistration.Pipeline;

public static class FluentValidationRegistrationExtensions
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
            typeof(IdentityApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);

        services.AddValidatorsFromAssembly(
            typeof(ReferenceApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);
        return services;
    }
}
