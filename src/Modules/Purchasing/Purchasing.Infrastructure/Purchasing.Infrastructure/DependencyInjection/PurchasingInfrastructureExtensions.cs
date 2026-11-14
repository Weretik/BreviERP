using Ai.Contracts.Prompts;
using Purchasing.Infrastructure.Adapters.Accounting;
using Purchasing.Infrastructure.Adapters.Ai;
using Purchasing.Infrastructure.Adapters.Reference;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Purchasing.Application.Abstractions;
using Purchasing.Application.DependencyInjection;
using Purchasing.Infrastructure.Persistence;
using Purchasing.Application.Features.Documents.Shared;
using Purchasing.Infrastructure.Integrations.Dropbox;
using Purchasing.Infrastructure.Integrations.GoogleSheets;
using Purchasing.Application.Abstractions.Sources;
using Purchasing.Application.Features.Documents.Shared.Recognition;
using BuildingBlocks.Infrastructure.Database;

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

        // Adapters from other modules' Contracts to this module's ports.
        services.AddScoped<IPurchasingSupplierReader, PurchasingSupplierReader>();
        services.AddScoped<IPurchasingPaymentReader, PurchasingPaymentReader>();
        services.AddScoped<IPurchaseDocumentRecognizer, PurchaseDocumentRecognizer>();
        services.AddSingleton<IAiBasePrompt, PurchaseInvoiceBasePrompt>();
        services.AddScoped<PurchaseFileRecognition>();
        return services;
    }
}
