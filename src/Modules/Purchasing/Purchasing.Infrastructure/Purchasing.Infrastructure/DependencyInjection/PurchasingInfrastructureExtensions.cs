using BuildingBlocks.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Purchasing.Application.Abstractions;
using Purchasing.Application.DependencyInjection;
using Purchasing.Infrastructure.Dropbox;
using Purchasing.Infrastructure.GoogleSheets;
using Purchasing.Infrastructure.Persistence;

namespace Purchasing.Infrastructure.DependencyInjection;

public static class PurchasingInfrastructureExtensions
{
    /// <summary>DbContext, migrator, Google Sheets reader and shared application services.</summary>
    public static IServiceCollection AddPurchasingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<PurchasingDbContext>(options => options.UseNpgsql(connection,
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", PurchasingDbContext.Schema)));
        services.AddScoped<IPurchasingDbContext>(sp => sp.GetRequiredService<PurchasingDbContext>());
        services.AddScoped<IDatabaseMigrator, DbMigrator<PurchasingDbContext>>();

        services.Configure<PurchaseSheetOptions>(configuration.GetSection(PurchaseSheetOptions.Section));
        services.AddSingleton<IGoogleAccessTokenSource, ApplicationDefaultTokenSource>();
        services.AddHttpClient<IPurchaseSheetReader, GooglePurchaseSheetReader>(client => client.Timeout = TimeSpan.FromSeconds(20));
        services.AddHttpClient<IPurchaseFileDownloader, DropboxFileDownloader>(client => client.Timeout = TimeSpan.FromSeconds(60));
        services.TryAddSingleton(TimeProvider.System);
        services.AddPurchasingApplication();
        return services;
    }
}
