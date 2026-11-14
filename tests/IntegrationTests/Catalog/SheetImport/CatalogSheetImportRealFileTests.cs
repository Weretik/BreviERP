using System.Text.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Enums;
using CatalogImport.Execution;
using CatalogImport.Persistence;
using Microsoft.EntityFrameworkCore;
using Reference.Domain.Suppliers;
using Catalog.Application.Features.Product.Commands.ImportSheetProducts;
using Reference.Infrastructure.Persistence;
using Catalog.Application.Features.Product.Commands.ImportSheetProducts.Report;

namespace IntegrationTests.Catalog.SheetImport;

/// <summary>
/// Opt-in rehearsal of the production transfer (CAT-010): the Calculator transfer files are imported through the
/// Reference import endpoints and then the product sheet import, all with dryRun false, against a throwaway
/// PostgreSQL database. Runs only when BREVIERP_SHEET_IMPORT_TRANSFER_DIR points at a folder with the transfer JSON
/// files (products-import.json, fabrics-import.json, accessories-import.json, operations-import.json,
/// product-sections-import.json, price-settings-import.json).
/// </summary>
[NonParallelizable]
public sealed class CatalogSheetImportRealFileTests
{
    private const string TransferDirVariable = "BREVIERP_SHEET_IMPORT_TRANSFER_DIR";

    /// <summary>PPE suppliers are not in the Reference transfer files; «3C-Макркет» is a sheet typo of «3C-Маркет».</summary>
    private static readonly string[] PpeSuppliers = ["3C-Маркет", "Інсайт", "АВ-Центр", "Аделія", "Перчатка-центр", "СЕМТО", "Трєле"];

    /// <param name="legacyCatalogue">First load the legacy catalogue fixture (CatalogImport job): its MOD-025 with
    /// articles 246/247 and PPE 724 are matched and updated, its other rows are deleted.</param>
    [TestCase(false, TestName = "Transfer_files_import_into_an_empty_database_and_a_repeat_is_unchanged")]
    [TestCase(true, TestName = "Transfer_files_import_over_the_legacy_catalogue_and_a_repeat_is_unchanged")]
    public async Task Transfer_files_import_and_a_repeat_is_unchanged(bool legacyCatalogue)
    {
        var folder = Environment.GetEnvironmentVariable(TransferDirVariable);
        if (string.IsNullOrWhiteSpace(folder))
            Assert.Ignore($"Set {TransferDirVariable} to the Calculator transfer folder to run this rehearsal.");

        await using var database = await SheetImportDatabase.CreateAsync("brevi_sheet_transfer");
        if (legacyCatalogue)
            await LoadLegacyCatalogueAsync(database.Connection);
        await using var host = await SheetImportHost.CreateAsync(database.Connection);
        await host.SeedSuppliersAsync(PpeSuppliers);
        foreach (var (file, path) in new[]
                 {
                     ("product-sections-import.json", "api/reference/product-sections/import"),
                     ("fabrics-import.json", "api/reference/fabrics/import"),
                     ("accessories-import.json", "api/reference/garment-accessories/import"),
                     ("operations-import.json", "api/reference/garment-part-operations/import"),
                     ("price-settings-import.json", "api/reference/pricing-parameters/import")
                 })
            await host.PostOkAsync(path, Applied(folder!, file));

        var products = Applied(folder!, "products-import.json");
        var preview = Applied(folder!, "products-import.json");
        preview["dryRun"] = true;

        var clock = Stopwatch.StartNew();
        var previewReport = await host.ImportAsync(preview);
        var previewTime = clock.Elapsed;
        Assert.That(previewReport.Problems, Is.Empty, Describe(previewReport.Problems));

        clock.Restart();
        var report = await host.ImportAsync(products);
        var importTime = clock.Elapsed;

        Assert.That(report.Problems, Is.Empty, Describe(report.Problems));
        Assert.Multiple(async () =>
        {
            Assert.That(report.Applied, Is.True);
            Assert.That(report.Sewing, Is.EqualTo(legacyCatalogue ? new SheetImportCounts(123, 1, 0) : new SheetImportCounts(124, 0, 0)));
            Assert.That(report.Ppe, Is.EqualTo(legacyCatalogue ? new SheetImportCounts(118, 1, 0) : new SheetImportCounts(119, 0, 0)));
            Assert.That(report.ToDelete, Is.Empty);
            Assert.That(await Count("Products", $"\"Type\" = {(int)ProductType.Sewing}"), Is.EqualTo(124));
            Assert.That(await Count("Products", $"\"Type\" = {(int)ProductType.Ppe}"), Is.EqualTo(119));
            Assert.That(await Count("SewingArticles"), Is.EqualTo(248));
            // MOD-174 «Флекс» shares one fabric in the sheet; it goes to both its variants (CAT-015).
            Assert.That(await Count("SewingMaterialLines"), Is.EqualTo(1449));
            Assert.That(await Count("SewingMaterialLines", "\"ArticleId\" IS NOT NULL"), Is.EqualTo(248));
            Assert.That(await Count("SewingArticles", "(SELECT count(*) FROM catalog.\"SewingMaterialLines\" l WHERE l.\"ArticleId\" = catalog.\"SewingArticles\".\"Id\" AND l.\"Kind\" = 1) <> 1"),
                Is.Zero, "every fabric variant has exactly its one fabric");
            Assert.That(await Count("SewingOperationLines"), Is.EqualTo(1823));
            Assert.That(await Count("SewingOperationLines", "\"ManualMinutes\" IS NOT NULL"), Is.EqualTo(33));
            Assert.That(await Count("SewingMaterialLines", "\"Comment\" = 'Спільна складова для всіх артикулів моделі'")
                + await Count("SewingOperationLines", "\"Comment\" = 'Спільна складова для всіх артикулів моделі'"), Is.Zero,
                "the note the sheet repeats on shared lines is dropped");
            Assert.That(await Count("PpePricingOfferVersions"), Is.EqualTo(118), "every PPE but 724 has a supplier price");
            Assert.That(await Count("SewingPricePublications"), Is.EqualTo(248), "every Article price is published by the import");
            Assert.That(await Count("ProductCatalogProfiles", $"\"PricingRuleCode\" IS NULL AND \"ProductId\" IN (SELECT \"Id\" FROM catalog.\"Products\" WHERE \"Type\" = {(int)ProductType.Ppe})"),
                Is.EqualTo(15), "«Одяг ЗІЗ» (14) and «Захист голови» (1) are not in «Налаштування цін»");
        });

        var after = await database.FingerprintAsync();
        clock.Restart();
        var repeat = await host.ImportAsync(Applied(folder!, "products-import.json"));
        var repeatTime = clock.Elapsed;
        Assert.Multiple(async () =>
        {
            Assert.That(repeat.Problems, Is.Empty, Describe(repeat.Problems));
            Assert.That(repeat.Sewing, Is.EqualTo(new SheetImportCounts(0, 0, 124)));
            Assert.That(repeat.Ppe, Is.EqualTo(new SheetImportCounts(0, 0, 119)));
            Assert.That(repeat.ToDelete, Is.Empty);
            Assert.That(await database.FingerprintAsync(), Is.EqualTo(after));
        });

        // The «Товари» list is one request without recalculation (CAT-013).
        await host.Client.GetAsync("api/v1/products/overview?type=Sewing&pageSize=25"); // warm-up
        clock.Restart();
        var sewingPage = await host.Client.GetFromJsonAsync<JsonElement>("api/v1/products/overview?type=Sewing&pageSize=25");
        var listTime = clock.Elapsed;
        clock.Restart();
        var ppePage = await host.Client.GetFromJsonAsync<JsonElement>("api/v1/products/overview?type=Ppe&pageSize=25&sort=price");
        var ppeListTime = clock.Elapsed;
        Assert.Multiple(() =>
        {
            Assert.That(sewingPage.GetProperty("total").GetInt32(), Is.EqualTo(124));
            Assert.That(sewingPage.GetProperty("items").GetArrayLength(), Is.EqualTo(25));
            Assert.That(sewingPage.GetProperty("withoutPrice").GetInt32(), Is.Zero, "every model has a published price");
            Assert.That(sewingPage.GetProperty("items").EnumerateArray().SelectMany(x => x.GetProperty("variants").EnumerateArray())
                .Select(x => x.GetProperty("fabricName").GetString()), Has.All.Not.Null.And.All.Not.Empty);
            Assert.That(ppePage.GetProperty("total").GetInt32(), Is.EqualTo(119));
        });

        TestContext.Progress.WriteLine(
            $"Sheet import (legacy catalogue {legacyCatalogue}): preview {previewTime.TotalMilliseconds:F0} ms, apply {importTime.TotalMilliseconds:F0} ms, " +
            $"repeat {repeatTime.TotalMilliseconds:F0} ms; list sewing {listTime.TotalMilliseconds:F0} ms, PPE {ppeListTime.TotalMilliseconds:F0} ms; warnings {report.Warnings.Count}: " +
            string.Join("; ", report.Warnings.GroupBy(x => x.Message).Select(x => $"{x.Count()}× {x.Key}")));

        Task<long> Count(string table, string? where = null)
            => database.ScalarAsync<long>($"SELECT count(*) FROM catalog.\"{table}\"" + (where is null ? "" : " WHERE " + where));
    }

    private static async Task LoadLegacyCatalogueAsync(string connection)
    {
        await using (var reference = new ReferenceDbContext(new DbContextOptionsBuilder<ReferenceDbContext>().UseNpgsql(connection).Options))
        {
            reference.Suppliers.Add(Supplier.Create(SupplierId.From(1), "Не указан").Value);
            await reference.SaveChangesAsync();
        }
        await using (var provenance = new CatalogImportDbContext(new DbContextOptionsBuilder<CatalogImportDbContext>().UseNpgsql(connection).Options))
            await provenance.Database.MigrateAsync();
        await CatalogImportJob.RunAsync(connection, Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "catalog-import.json"), "apply");
    }

    private static JsonObject Applied(string folder, string file)
    {
        var body = JsonNode.Parse(File.ReadAllText(Path.Combine(folder, file)))!.AsObject();
        body["dryRun"] = false;
        return body;
    }

    private static string Describe(IEnumerable<SheetImportIssue> issues)
        => string.Join("; ", issues.Take(30).Select(x => $"{x.Scope} {x.RowNumber} {x.Code}: {x.Message}"));
}
