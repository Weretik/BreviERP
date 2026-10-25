using System.Data;
using System.Net;
using System.Text.Json;
using Catalog.Application.Features.Pricing.Sewing.Preview;
using Catalog.Domain.Pricing.Sewing;
using Catalog.Domain.Products.Entities;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.Reading;
using CatalogImport.Persistence;
using SewingPricing.Jobs.Reading;
using CatalogImport.Source;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Reference.Infrastructure.DataBase;
using Reference.Infrastructure.Pricing.Reading;

namespace SewingPricing.Jobs.Verification;

public static class SewingPricingVerificationJob
{
    public static async Task<IReadOnlyList<SewingPricingDiagnostic>> RunAsync(
        string connectionString, IReadOnlyList<string> saleSkus, DateTimeOffset at, CancellationToken cancellationToken = default)
    {
        var target = new NpgsqlConnectionStringBuilder(connectionString);
        if (!(target.Host == "localhost" || IPAddress.TryParse(target.Host, out var address) && IPAddress.IsLoopback(address)))
            throw new InvalidOperationException("Pricing verification only supports a local PostgreSQL target.");
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
        await using var catalog = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(connection).Options);
        await using var reference = new ReferenceDbContext(new DbContextOptionsBuilder<ReferenceDbContext>().UseNpgsql(connection).Options);
        await using var provenance = new CatalogImportDbContext(new DbContextOptionsBuilder<CatalogImportDbContext>().UseNpgsql(connection).Options);
        await catalog.Database.UseTransactionAsync(transaction, cancellationToken);
        await reference.Database.UseTransactionAsync(transaction, cancellationToken);
        await provenance.Database.UseTransactionAsync(transaction, cancellationToken);
        var batch = await provenance.Batches.AsNoTracking().SingleAsync(cancellationToken);
        var source = JsonSerializer.Deserialize<WorkbookSnapshot>(batch.SourceJson, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        var inputReader = new SewingPricingInputReader(catalog);
        var settingsReader = new SewingPricingSettingsReader(catalog);
        var referenceReader = new JobPricingReferenceReader(new ReferencePricingVersionReader(reference));
        var reader = new SewingArticleDiagnosticReader(catalog, source, inputReader, settingsReader, referenceReader);
        var reports = new List<SewingPricingDiagnostic>();
        foreach (var sku in saleSkus)
            reports.Add(await reader.ReadAsync(sku, at, cancellationToken));
        return reports;
    }

}
