using Accounting.Application;
using Crm.Application;
using Catalog.Application;
using FluentValidation;
using Identity.Application;
using Reference.Application;
using Reminders.Application;

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
            typeof(CatalogApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);

        services.AddValidatorsFromAssembly(
            typeof(ReferenceApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);

        services.AddValidatorsFromAssembly(
            typeof(RemindersApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);
        return services;
    }
}
