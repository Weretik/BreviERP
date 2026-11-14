using BuildingBlocks.Application.DependencyInjection;
using Reference.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reference.Infrastructure.DependencyInjection;

namespace Reference.Api.DependencyInjection;

public static class ReferenceModuleExtensions
{
    /// <summary>Registers the Reference module: Application, Infrastructure, its Contracts implementations and controllers.</summary>
    public static IServiceCollection AddReferenceModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReferenceInfrastructure(configuration);
        services.AddModuleValidators(typeof(ReferenceApplicationAssemblyMarker).Assembly);
        services.AddControllers().AddApplicationPart(typeof(ReferenceApiAssemblyMarker).Assembly);
        return services;
    }
}
