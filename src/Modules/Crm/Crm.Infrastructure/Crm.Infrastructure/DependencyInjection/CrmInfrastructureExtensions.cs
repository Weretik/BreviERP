using BuildingBlocks.Infrastructure.Migrations;
using Crm.Application.Customers.Persistence;
using Crm.Application.Quotes.Building;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Tasks.Persistence;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Customers.Reading;
using Crm.Infrastructure.Persistence.Customers.Writing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Crm.Infrastructure.DependencyInjection;

public static class CrmInfrastructureExtensions
{
    public static IServiceCollection AddCrmDbContextServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<CrmDbContext>(options => options.UseNpgsql(connection, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "crm")));
        services.AddScoped<ICustomerReadRepository, CustomerReadRepository>();
        services.AddScoped<ICustomerMutationStore, CustomerMutationStore>();
        services.AddScoped<IQuoteStore>(provider => provider.GetRequiredService<CrmDbContext>());
        services.AddScoped<ITaskStore>(provider => provider.GetRequiredService<CrmDbContext>());
        services.AddScoped<QuoteItemsBuilder>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IDatabaseMigrator, DbMigrator<CrmDbContext>>();
        return services;
    }
}
