using BuildingBlocks.Application;
using BuildingBlocks.Application.DependencyInjection;
using Identity.Application;
using Reference.Application;

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
                typeof(ReferenceApplicationAssemblyMarker).Assembly,
                typeof(ApplicationAssemblyMarker).Assembly
            ];

            options.PipelineBehaviors = MediatorPipeline.PipelineBehaviors;
        });

        return services;
    }
}
