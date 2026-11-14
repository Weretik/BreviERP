using Identity.Application.Security.Access;
using Identity.Contracts.Staff;
using Identity.Infrastructure.Adapters.Media;
using Identity.Infrastructure.Contracts.Staff;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Identity.Application.Abstractions;
using Identity.Infrastructure.Integrations.AspNetIdentity;
using Identity.Infrastructure.Integrations.AspNetIdentity.Access;
using Identity.Infrastructure.Integrations.AspNetIdentity.Profile;
using Identity.Infrastructure.Integrations.AspNetIdentity.Sessions;
using Identity.Infrastructure.Integrations.AspNetIdentity.Users;
using Identity.Infrastructure.Integrations.Avatars;
using Identity.Infrastructure.Persistence.Seeding;

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
        services.AddScoped<IProfileService, ProfileService>();
        // «Права доступу»: replaces the defaults-only source Host.Api registers for its authorization handler.
        services.AddMemoryCache();
        services.AddScoped<IAccessGrantSource, AccessGrantSource>();
        services.AddScoped<IAccessRightsService, AccessRightsService>();
        services.AddSingleton<AvatarImageProcessor>();
        services.AddScoped<AvatarService>();
        // Avatars live on the Media CDN; without its sign-in they are switched off.
        // TryAdd: a host or test may register its own avatar storage first.
        services.TryAddSingleton<IAvatarStorage, CdnAvatarStorage>();

        // Contracts other modules use (ADR-0006).
        services.AddScoped<IStaffDirectory, StaffDirectory>();
        return services;
    }

    // Backward-compatible alias for existing callers in this repository.
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        => services.AddIdentityInfrastructureServices(configuration);
}
