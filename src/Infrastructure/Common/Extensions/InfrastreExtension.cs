using Application.Identity.Interfaces;
using Application.Reference.Shared;
using Infrastructure.Common.Contracts;
using Infrastructure.Common.Migrator;
using Infrastructure.Identity;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Repositories;
using Infrastructure.Identity.Seeders;
using Infrastructure.Reference;
using Infrastructure.Reference.Repositories;
using Infrastructure.Reference.Seeders;

namespace Infrastructure.Common.Extensions;

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
        // Repository
        services.AddScoped(typeof(IReferenceRepository<>), typeof(ReferenceEfRepository<>));
        services.AddScoped(typeof(IReferenceReadRepository<>), typeof(ReferenceReadEfRepository<>));
        // Migrator
        services.AddScoped<IDatabaseMigrator, DbMigrator<ReferenceDbContext>>();

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

        // Repository
        services.AddScoped(typeof(IAppIdentityRepository<>), typeof(AppIdentityEfRepository<>));
        services.AddScoped(typeof(IAppIdentityReadRepository<>), typeof(AppIdentityEfRepository<>));
        // Migrator
        services.AddScoped<IDatabaseMigrator, DbMigrator<AppIdentityDbContext>>();

        // Infrastructure Services
        services.AddScoped<IDomainEventContext, EfDomainEventContext>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        // Seeders
        services.AddScoped<ISeeder, AdditionalReferenceSeeder>();
        services.AddScoped<ISeeder, RoleSeeder>();
        services.AddScoped<ISeeder, IdentitySeeder>();

        return services;
    }
}
