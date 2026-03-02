using BuildingBlocks.Infrastructure.Migrations;
using BuildingBlocks.Infrastructure.Seeding;
using Reference.Application.Contracts.Persistence;
using Reference.Infrastructure.DataBase;
using Reference.Infrastructure.Repositories;
using Reference.Infrastructure.Seeders;

namespace Reference.Infrastructure.DependencyInjection;

public static class ReferenceDbContextExtensions
{
    public static IServiceCollection AddReferenceDbContextServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");;

        services.AddDbContext<ReferenceDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IReadReferenceDbContext>(sp => sp.GetRequiredService<ReferenceDbContext>());

        services.AddScoped(typeof(IReferenceRepository<>), typeof(ReferenceEfRepository<>));
        services.AddScoped(typeof(IReferenceReadRepository<>), typeof(ReferenceReadEfRepository<>));

        services.AddScoped<IDatabaseMigrator, DbMigrator<ReferenceDbContext>>();
        services.AddScoped<ISeeder, AdditionalReferenceSeeder>();
        services.AddScoped<ISeeder, FabricSeeder>();

        return services;
    }
}
