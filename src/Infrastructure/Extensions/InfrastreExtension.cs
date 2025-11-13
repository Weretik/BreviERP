using Application.Identity.Interfaces;
using Application.Reference.Shared;
using Infrastructure.Common.Contracts;
using Infrastructure.Identity;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Interfaces;
using Infrastructure.Identity.Migrations;
using Infrastructure.Identity.Persistence;
using Infrastructure.Identity.Security;
using Infrastructure.Reference;
using Infrastructure.Reference.Repositories;

namespace Infrastructure.Extensions;

public static class InfrastreExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Connections DB
        var connectionString = configuration.GetConnectionString("Default");

        // Reference
        // write operations
        services.AddDbContext<ReferenceDbContext>(options => options.UseNpgsql(connectionString));
        // read operations
        services.AddDbContextFactory<ReferenceDbContext>(options => options.UseNpgsql(connectionString),
            lifetime: ServiceLifetime.Scoped);
        //Repository
        services.AddScoped(typeof(IReferenceRepository<>), typeof(ReferenceEfRepository<>));
        services.AddScoped(typeof(IReferenceReadRepository<>), typeof(ReferenceReadEfRepository<>));

        // Registration of migrants identification
        services.AddScoped<IDatabaseMigrator, AppIdentityDbMigrator>();
        services.AddScoped<IAppIdentityDbMigrator, AppIdentityDbMigrator>();

        //Fake Services
        services.AddScoped<IPermissionService, FakePermissionService>();
        services.AddScoped<ICurrentUserService, FakeCurrentUserService>();

        // Infrastructure Services
        services.AddSingleton<IEnvironmentService, EnvironmentService>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        // Identity DB
        services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        //Repository
        services.AddScoped(typeof(IAppIdentityRepository<>), typeof(AppIdentityEfRepository<>));
        services.AddScoped(typeof(IAppIdentityReadRepository<>), typeof(AppIdentityEfRepository<>));

        return services;
    }
}
