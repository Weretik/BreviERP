using System.Net;
using System.Text.Json;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Domain.Products.Enums;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.Ppe.Reading;
using CatalogImport.Persistence;
using CatalogImport.Source;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CatalogImport.PpePricing.Verification;

internal static class PpePricingVerificationReader
{
    public static async Task<PpePricingVerificationData> ReadAsync(string connectionString, CancellationToken cancellationToken)
    {
        var target = new NpgsqlConnectionStringBuilder(connectionString);
        if (!(target.Host == "localhost" || IPAddress.TryParse(target.Host, out var address) && IPAddress.IsLoopback(address)))
            throw new InvalidOperationException("PPE verification only supports a local PostgreSQL target.");
        await using var provenance = new CatalogImportDbContext(new DbContextOptionsBuilder<CatalogImportDbContext>().UseNpgsql(connectionString).Options);
        var batch = await provenance.Batches.AsNoTracking().SingleAsync(cancellationToken);
        var source = JsonSerializer.Deserialize<WorkbookSnapshot>(batch.SourceJson, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        await using var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(connectionString).Options);
        var products = await db.Products.AsNoTracking().Where(x => x.Type == ProductType.Ppe && x.CatalogProfile != null)
            .Select(x => new { Id = x.Id.Value, Sku = x.CatalogProfile!.SaleSku }).ToListAsync(cancellationToken);
        var ids = products.Where(x => x.Sku != null).ToDictionary(x => x.Sku!, x => x.Id, StringComparer.Ordinal);
        var rows = source.Rows("Прайс на сайт").Where(x => x.Text(5) == "ЗІЗ").ToArray();
        var inputs = new Dictionary<int, PpePricingProductInput>();
        var reader = new PpePricingInputReader(db);
        foreach (var chunk in rows.Select(x => ids.GetValueOrDefault(x.Text(0))).Where(x => x > 0).Distinct().Chunk(100))
            foreach (var input in await reader.ReadAsync(chunk, cancellationToken))
                inputs.Add(input.ProductId, input);
        return new(batch.SpreadsheetId, batch.SnapshotHash, source.CapturedAt, rows,
            ids.Where(x => inputs.ContainsKey(x.Value)).ToDictionary(x => x.Key, x => inputs[x.Value], StringComparer.Ordinal));
    }
}
