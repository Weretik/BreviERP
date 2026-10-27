using Crm.Infrastructure.Persistence.Customers.Writing;
using Crm.Infrastructure.Persistence.Customers.Reading;
using BuildingBlocks.Infrastructure.Migrations;
using Crm.Application.Customers.Persistence;
using Crm.Application.Quotes.Pricing;
using Crm.Infrastructure.Persistence.Quotes.Pricing;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Mapping;
using Crm.Infrastructure.Persistence.Quotes.Writing;
using Crm.Infrastructure.Persistence.Quotes.Reading;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Crm.Infrastructure.DependencyInjection;

public static class CrmInfrastructureExtensions
{
    public static IServiceCollection AddCrmDbContextServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<CrmDbContext>(options => options.UseNpgsql(connection, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "crm")));
        services.AddScoped<ICustomerReadRepository, CustomerReadRepository>();
        services.AddScoped<ICustomerMutationStore, CustomerMutationStore>();
        services.AddScoped<IQuotePriceBasisStore, QuotePriceBasisStore>();
        services.AddScoped<IQuoteMutationStore, QuoteMutationStore>();
        services.AddScoped<IQuoteReadRepository, QuoteReadRepository>();
        services.AddScoped<QuoteVersionBuilder>();
        services.AddScoped<QuotePriceBasisResolver>();
        services.AddScoped<IDatabaseMigrator, DbMigrator<CrmDbContext>>();
        return services;
    }
}
