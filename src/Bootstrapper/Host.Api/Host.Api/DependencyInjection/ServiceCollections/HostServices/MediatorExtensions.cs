using Accounting.Application;
using BuildingBlocks.Application.Behaviors;
using Crm.Api;
using Reference.Api;

namespace Host.Api.DependencyInjection.ServiceCollections.HostServices;

public static class MediatorExtensions
{
    public static IServiceCollection AddMediatorPipeline(
        this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;

            options.Assemblies =
            [
                typeof(AccountingApplicationAssemblyMarker).Assembly,
                typeof(CrmApiAssemblyMarker).Assembly,
                typeof(ReferenceApiAssemblyMarker).Assembly
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
