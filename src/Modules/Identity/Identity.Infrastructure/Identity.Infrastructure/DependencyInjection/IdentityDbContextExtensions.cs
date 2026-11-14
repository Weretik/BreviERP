using Microsoft.AspNetCore.DataProtection;
using Identity.Infrastructure.Integrations.AspNetIdentity;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Seeding;
using Identity.Infrastructure.Persistence.Configurations.Users;
using BuildingBlocks.Infrastructure.Database;

namespace Identity.Infrastructure.DependencyInjection;

internal static class IdentityDbContextExtensions
{
    internal static IServiceCollection AddIdentityDbContextServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppIdentityDbContext>());

        // Keys live in PostgreSQL so session cookies survive deploys and work on every Cloud Run instance.
        services
            .AddDataProtection()
            .PersistKeysToDbContext<AppIdentityDbContext>()
            .SetApplicationName("BreviERP");

        services.AddIdentityAuthentication(configuration);

        services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<AppRole>()
            .AddSignInManager()
            .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IIdentitySeeder, RoleSeeder>();
        services.AddScoped<IIdentitySeeder, IdentitySeeder>();

        services.AddScoped<IDatabaseMigrator, DbMigrator<AppIdentityDbContext>>();

        services.AddScoped<ISeeder, RoleSeeder>();
        services.AddScoped<ISeeder, IdentitySeeder>();

        return services;
    }
}
