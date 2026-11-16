using BuildingBlocks.Application.Actors;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Api.Authorization;

public static class CurrentActorRegistrationExtensions
{
    /// <summary>The request's signed-in user as <see cref="ICurrentActor"/>; call before the modules register.</summary>
    public static IServiceCollection AddHttpCurrentActor(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentActor, HttpCurrentActor>();
        return services;
    }
}
