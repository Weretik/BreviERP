using Identity.Application.Security.Access;
using Identity.Application.Services;
using Identity.Infrastructure.Avatars;
using Identity.Infrastructure.Options;
using Identity.Infrastructure.Services.Access;
using Identity.Infrastructure.Services.Profile;
using Identity.Infrastructure.Services.Sessions;
using Identity.Infrastructure.Services.Users;

using Microsoft.Extensions.DependencyInjection.Extensions;

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
        // Host.Api registers the CDN storage first; elsewhere avatars are switched off.
        services.TryAddSingleton<IAvatarStorage, UnconfiguredAvatarStorage>();
        return services;
    }

    // Backward-compatible alias for existing callers in this repository.
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        => services.AddIdentityInfrastructureServices(configuration);
}
