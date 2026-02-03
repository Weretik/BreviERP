using BuildingBlocks.Infrastructure.Migrations;
using Reference.Application.Contracts.Persistence;
using Reference.Infrastructure.DataBase;
using Reference.Infrastructure.Repositories;

namespace Reference.Infrastructure.DependencyInjection;

public static class ReferenceInfrastructureExtensions
{
    public static IServiceCollection AddReferenceInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");;

        services.AddDbContext<ReferenceDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IReadReferenceDbContext>(sp => sp.GetRequiredService<ReferenceDbContext>());

        services.AddScoped(typeof(IReferenceRepository<>), typeof(ReferenceEfRepository<>));
        services.AddScoped(typeof(IReferenceReadRepository<>), typeof(ReferenceReadEfRepository<>));

        services.AddScoped<IDatabaseMigrator, DbMigrator<ReferenceDbContext>>();


        // Migrator
        services.AddScoped<IDatabaseMigrator, DbMigrator<AppIdentityDbContext>>();
        services.AddScoped<IDatabaseMigrator, DbMigrator<CrmDbContext>>();
        // Seeders
        services.AddScoped<ISeeder, AdditionalReferenceSeeder>();
        services.AddScoped<ISeeder, CounterpartySeeder>();
        services.AddScoped<ISeeder, FabricSeeder>();

        return services;
    }
}
