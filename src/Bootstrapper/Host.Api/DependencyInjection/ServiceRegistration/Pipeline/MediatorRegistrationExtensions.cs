using Accounting.Application;
using BuildingBlocks.Application.Behaviors;
using Crm.Application;
using Identity.Application;
using Reference.Application;

namespace Host.Api.DependencyInjection.ServiceRegistration.Pipeline;

public static class MediatorRegistrationExtensions
{
    public static IServiceCollection AddMediatorPipeline(
        this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.Assemblies =
            [
                typeof(AccountingApplicationAssemblyMarker).Assembly,
                typeof(CrmApplicationAssemblyMarker).Assembly,
                typeof(IdentityApplicationAssemblyMarker).Assembly,
                typeof(ReferenceApplicationAssemblyMarker).Assembly
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
