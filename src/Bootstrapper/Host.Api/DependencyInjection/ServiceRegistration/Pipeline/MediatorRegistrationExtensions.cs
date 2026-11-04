using BuildingBlocks.Application;
using Catalog.Application;
using Crm.Application;
using BuildingBlocks.Application.Behaviors;
using Identity.Application;
using Reference.Application;
using Reminders.Application;
using Media.Application;
using Production.Application;

namespace Host.Api.DependencyInjection.ServiceRegistration.Pipeline;

public static class MediatorRegistrationExtensions
{
    public static IServiceCollection AddMediatorPipeline(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;

            options.Assemblies =
            [
                typeof(IdentityApplicationAssemblyMarker).Assembly,
                typeof(CatalogApplicationAssemblyMarker).Assembly,
                typeof(CrmApplicationAssemblyMarker).Assembly,
                typeof(ReferenceApplicationAssemblyMarker).Assembly,
                typeof(RemindersApplicationAssemblyMarker).Assembly,
                typeof(MediaApplicationAssemblyMarker).Assembly,
                typeof(ProductionApplicationAssemblyMarker).Assembly,
                typeof(ApplicationAssemblyMarker).Assembly
            ];

            options.PipelineBehaviors =
            [
                typeof(RequestLoggingBehavior<,>),
                typeof(PerformanceBehavior<,>),
                typeof(ValidationBehavior<,>),
                typeof(ExceptionBehavior<,>),
                typeof(DomainEventDispatcherBehavior<,>)
            ];
        });

        return services;
    }
}
