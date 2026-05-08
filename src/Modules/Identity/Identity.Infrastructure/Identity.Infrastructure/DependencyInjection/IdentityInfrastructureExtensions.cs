using Identity.Application.Services;
using Identity.Infrastructure.Services;

namespace Identity.Infrastructure.DependencyInjection;

public static class IdentityInfrastructureExtensions
{
    public static IServiceCollection AddIdentityInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityDbContextServices(configuration);
        services.AddScoped<IIdentitySessionService, IdentitySessionService>();
        return services;
    }

    // Backward-compatible alias for existing callers in this repository.
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        => services.AddIdentityInfrastructureServices(configuration);
}
