using Accounting.Application.Abstractions;
using Accounting.Infrastructure.Persistence;
using BuildingBlocks.Infrastructure.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Accounting.Infrastructure.DependencyInjection;

public static class AccountingInfrastructureExtensions
{
    /// <summary>DbContext (schema accounting with its own migration history) and its migrator.</summary>
    public static IServiceCollection AddAccountingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<AccountingDbContext>(options => options.UseNpgsql(connection,
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", AccountingDbContext.Schema)));
        services.AddScoped<IAccountingDbContext>(sp => sp.GetRequiredService<AccountingDbContext>());
        services.AddScoped<IDatabaseMigrator, DbMigrator<AccountingDbContext>>();
        services.TryAddSingleton(TimeProvider.System);
        return services;
    }
}
