using Application.CRM.Shared.Contracts;
using Application.Identity.Interfaces;
using Application.Reference.Shared;
using Infrastructure.Common.Contracts;
using Infrastructure.Common.Migrator;
using Infrastructure.CRM;
using Infrastructure.CRM.Repositories;
using Infrastructure.CRM.Seeders;
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
        // Contexts
        // write operations
        services.AddDbContext<ReferenceDbContext>(options => options.UseNpgsql(connectionString));
        services.AddDbContext<CrmDbContext>(options => options.UseNpgsql(connectionString));
        // read operations
        services.AddDbContextFactory<ReferenceDbContext>(options => options.UseNpgsql(connectionString),
            lifetime: ServiceLifetime.Scoped);
        services.AddDbContextFactory<CrmDbContext>(options => options.UseNpgsql(connectionString),
            lifetime: ServiceLifetime.Scoped);

        // Identity DB
        services.AddDbContext<AppIdentityDbContext>(options => options.UseNpgsql(connectionString));

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

        // Migrator
        services.AddScoped<IDatabaseMigrator, DbMigrator<AppIdentityDbContext>>();
        services.AddScoped<IDatabaseMigrator, DbMigrator<ReferenceDbContext>>();
        services.AddScoped<IDatabaseMigrator, DbMigrator<CrmDbContext>>();

        // Seeders
        services.AddScoped<ISeeder, RoleSeeder>();
        services.AddScoped<ISeeder, IdentitySeeder>();
        services.AddScoped<ISeeder, AdditionalReferenceSeeder>();
        services.AddScoped<ISeeder, CounterpartySeeder>();

        // Repository
        services.AddScoped(typeof(IAppIdentityRepository<>), typeof(AppIdentityEfRepository<>));
        services.AddScoped(typeof(IAppIdentityReadRepository<>), typeof(AppIdentityEfRepository<>));

        services.AddScoped(typeof(IReferenceRepository<>), typeof(ReferenceEfRepository<>));
        services.AddScoped(typeof(IReferenceReadRepository<>), typeof(ReferenceReadEfRepository<>));

        services.AddScoped(typeof(ICounterpartyRepository<>), typeof(CounterpartyEfRepository<>));
        services.AddScoped(typeof(ICounterpartyReadRepository<>), typeof(CounterpartyReadEfRepository<>));

        // Infrastructure Services
        services.AddScoped<IDomainEventContext, EfDomainEventContext>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        return services;
    }
}
