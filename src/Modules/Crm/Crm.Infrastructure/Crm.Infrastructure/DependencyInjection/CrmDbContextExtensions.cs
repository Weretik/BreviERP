using BuildingBlocks.Infrastructure.Migrations;
using Crm.Application.Contracts;
using Crm.Infrastructure.Repositories;
using Crm.Infrastructure.Seeders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Infrastructure.DependencyInjection;

public static class CrmDbContextExtensions
{
    public static IServiceCollection AddCrmDbContextServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");;

        services.AddDbContext<CrmDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IReadCrmDbContext>(sp => sp.GetRequiredService<CrmDbContext>());

        services.AddScoped(typeof(ICrmRepository<>), typeof(CrmEfRepository<>));
        services.AddScoped(typeof(ICrmReadRepository<>), typeof(CrmReadEfRepository<>));

        services.AddScoped<IDatabaseMigrator, DbMigrator<CrmDbContext>>();
        services.AddScoped<ISeeder, CounterpartySeeder>();

        return services;
    }
}
