using Identity.Application.Services;
using Identity.Infrastructure.Options;
using Identity.Infrastructure.Services.Sessions;
using Identity.Infrastructure.Services.Users;

namespace Identity.Infrastructure.DependencyInjection;

public static class IdentityInfrastructureExtensions
{
    public static IServiceCollection AddIdentityInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityDbContextServices(configuration);
        // Bound here so both Host.Api and the seed job, which runs IdentitySeeder, get the administrator settings.
        services.Configure<AdminUserOptions>(configuration.GetSection(AdminUserOptions.SectionName));
        services.Configure<IdentityCookieSessionOptions>(configuration.GetSection(IdentityCookieSessionOptions.SectionName));
        services.AddScoped<ICookieSessionService, CookieSessionService>();
        services.AddScoped<IStaffAccountService, StaffAccountService>();
        return services;
    }

    // Backward-compatible alias for existing callers in this repository.
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        => services.AddIdentityInfrastructureServices(configuration);
}
