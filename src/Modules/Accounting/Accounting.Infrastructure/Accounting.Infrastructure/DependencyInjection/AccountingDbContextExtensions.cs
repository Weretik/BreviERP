using Accounting.Application.Contracts.Persistence;
using Accounting.Infrastructure.DataBase;
using Accounting.Infrastructure.Repositories;
using BuildingBlocks.Infrastructure.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Accounting.Infrastructure.DependencyInjection;

public static class AccountingDbContextExtensions
{
    public static IServiceCollection AddAccountingDbContextServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");;

        services.AddDbContext<AccountingDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IReadAccountingDbContext>(sp => sp.GetRequiredService<AccountingDbContext>());

        services.AddScoped(typeof(IAccountingRepository<>), typeof(AccountingEfRepository<>));
        services.AddScoped(typeof(IAccountingReadRepository<>), typeof(AccountingReadEfRepository<>));

        services.AddScoped<IDatabaseMigrator, DbMigrator<AccountingDbContext>>();

        return services;
    }
}
