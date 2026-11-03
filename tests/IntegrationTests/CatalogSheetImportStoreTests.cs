using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Catalog.Application.Features.Product.SheetImport.DTOs;
using Catalog.Domain.Products.Enums;
using Catalog.Infrastructure.DataBase;
using Host.Api.DependencyInjection.ServiceRegistration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Reference.Domain.Suppliers.Entities;
using Reference.Domain.Suppliers.ValueObjects;
using Reference.Infrastructure.DataBase;
using BuildingBlocks.Domain.ValueObjects;

namespace IntegrationTests;

/// <summary>
/// CAT-010 against PostgreSQL: the sheet import endpoint runs through the real host services (Reference code reader,
/// PPE rule reader, store and offer writer) and writes in one transaction.
/// </summary>
[NonParallelizable]
public sealed class CatalogSheetImportStoreTests
{
    private const string FailingSupplierPrice = "777.77";
    private SheetImportDatabase _database = null!;

    [SetUp]
    public async Task CreateDatabase() => _database = await SheetImportDatabase.CreateAsync("brevi_sheet_import");

    [TearDown]
    public async Task DropDatabase()
    {
        if (_database is not null) await _database.DisposeAsync();
    }

    [Test]
    public async Task Preview_writes_nothing_import_saves_everything_and_a_repeat_is_unchanged()
    {
        await using var host = await SheetImportHost.CreateAsync(_database.Connection);
        await SeedReferenceAsync(host);

        var seeded = await host.ImportAsync(new { dryRun = false, deleteMissing = false, sewing = new[] { OldSewing() }, ppe = new[] { OldPpe() } });
        Assert.That(seeded.Applied, Is.True, Describe(seeded));
        var oldPpeId = await _database.ScalarAsync<int>("SELECT p.\"Id\" FROM catalog.\"Products\" p JOIN catalog.\"ProductCatalogProfiles\" c ON c.\"ProductId\" = p.\"Id\" WHERE c.\"SaleSku\" = '500'");
        var oldSewingId = await _database.ScalarAsync<int>("SELECT \"Id\" FROM catalog.\"Products\" WHERE \"ModelCode\" = 'MOD-900'");
        Assert.That(await OfferCountAsync(oldPpeId), Is.EqualTo(1));

        var before = await _database.FingerprintAsync();
        var preview = await host.ImportAsync(Body(dryRun: true));
        Assert.Multiple(() =>
        {
            Assert.That(preview.Applied, Is.False);
            Assert.That(preview.Problems, Is.Empty, Describe(preview));
            Assert.That(preview.Sewing, Is.EqualTo(new SheetImportCounts(2, 0, 0)));
            Assert.That(preview.Ppe, Is.EqualTo(new SheetImportCounts(3, 1, 0)));
            Assert.That(preview.ToDelete.Select(x => x.Id), Is.EqualTo(new[] { oldSewingId }));
        });
        Assert.That(await _database.FingerprintAsync(), Is.EqualTo(before), "A preview must not write.");

        var applied = await host.ImportAsync(Body(dryRun: false));
        Assert.That(applied.Applied, Is.True, Describe(applied));
        Assert.That(applied.Warnings.Select(x => x.Code), Is.SupersetOf(new[] { "502", "503" }));

        Assert.Multiple(async () =>
        {
            Assert.That(await _database.ScalarAsync<long>("SELECT count(*) FROM catalog.\"Products\" WHERE \"Id\" = " + oldSewingId), Is.Zero);
            Assert.That(await _database.ScalarAsync<long>("SELECT count(*) FROM catalog.\"Products\" WHERE \"Type\" = " + (int)ProductType.Sewing), Is.EqualTo(2));
            Assert.That(await _database.ScalarAsync<long>("SELECT count(*) FROM catalog.\"Products\" WHERE \"Type\" = " + (int)ProductType.Ppe), Is.EqualTo(4));
            Assert.That(await _database.ScalarAsync<string>("SELECT \"Name\" FROM catalog.\"Products\" WHERE \"Id\" = " + oldPpeId), Is.EqualTo("Рукавички робочі нові"));
            Assert.That(await _database.ScalarAsync<long>("SELECT count(*) FROM catalog.\"SewingArticles\""), Is.EqualTo(4));
            Assert.That(await LinesAsync("MOD-101", "SewingMaterialLines"), Is.EqualTo(3), "shared fabric + one accessory per article");
            Assert.That(await LinesAsync("MOD-101", "SewingOperationLines"), Is.EqualTo(3), "OP-001 twice and OP-002");
            Assert.That(await _database.ScalarAsync<long>("SELECT count(*) FROM catalog.\"SewingMaterialLines\" m JOIN catalog.\"Products\" p ON p.\"Id\" = m.\"ProductId\" WHERE p.\"ModelCode\" = 'MOD-101' AND m.\"ArticleId\" IS NOT NULL"), Is.EqualTo(2));
            Assert.That(await _database.ScalarAsync<long>("SELECT count(*) FROM catalog.\"SewingOperationLines\" o JOIN catalog.\"Products\" p ON p.\"Id\" = o.\"ProductId\" WHERE p.\"ModelCode\" = 'MOD-101' AND o.\"ManualMinutes\" = 4.5"), Is.EqualTo(1));
            Assert.That(await ReviewedAsync("MOD-101"), Is.EqualTo((6L, 6L)), "a checked model has every line reviewed");
            Assert.That(await ReviewedAsync("MOD-102"), Is.EqualTo((0L, 2L)), "an unchecked model has no reviewed line");
            Assert.That(await OfferCountAsync(oldPpeId), Is.EqualTo(2), "the matched PPE gets a second offer version");
            Assert.That(await _database.ScalarAsync<long>("SELECT count(*) FROM catalog.\"PpePricingOfferVersions\""), Is.EqualTo(4), "500 (2) + 501 + 502 (without rule); 503 has no price");
            Assert.That(await _database.ScalarAsync<string?>("SELECT \"PricingRuleCode\" FROM catalog.\"ProductCatalogProfiles\" WHERE \"SaleSku\" = '502'"), Is.Null, "unknown rule is dropped");
            Assert.That(await _database.ScalarAsync<string?>("SELECT \"PricingRuleCode\" FROM catalog.\"ProductCatalogProfiles\" WHERE \"SaleSku\" = '501'"), Is.EqualTo("Рукавички тест"));
        });

        // The admin card loads every imported product by id.
        foreach (var id in await _database.IdsAsync("SELECT \"Id\" FROM catalog.\"Products\" ORDER BY \"Id\""))
        {
            var detail = await host.Client.GetAsync($"api/v1/products/{id}");
            Assert.That(detail.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"product {id}: {await detail.Content.ReadAsStringAsync()}");
        }

        // The sewing card loads the shared and every Article's composition.
        foreach (var (productId, articleId) in await _database.PairsAsync(
                     "SELECT \"ProductId\", \"Id\" FROM catalog.\"SewingArticles\" ORDER BY \"Id\""))
        {
            var shared = await host.Client.GetAsync($"api/v1/products/{productId}/composition");
            Assert.That(shared.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"composition {productId}: {await shared.Content.ReadAsStringAsync()}");
            var article = await host.Client.GetAsync($"api/v1/products/{productId}/composition?articleId={articleId}");
            Assert.That(article.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"composition {productId}/{articleId}: {await article.Content.ReadAsStringAsync()}");
        }

        var ppe501 = await _database.ScalarAsync<int>("SELECT p.\"Id\" FROM catalog.\"Products\" p JOIN catalog.\"ProductCatalogProfiles\" c ON c.\"ProductId\" = p.\"Id\" WHERE c.\"SaleSku\" = '501'");
        var offer = await host.Client.GetStringAsync($"api/v1/products/{ppe501}/ppe-offer");
        Assert.That(offer, Does.Contain("\"isCalculated\":true").And.Contain("\"retail\":120"), "the PPE card shows the three prices");

        var after = await _database.FingerprintAsync();
        var repeat = await host.ImportAsync(Body(dryRun: false));
        Assert.Multiple(async () =>
        {
            Assert.That(repeat.Problems, Is.Empty, Describe(repeat));
            Assert.That(repeat.Sewing, Is.EqualTo(new SheetImportCounts(0, 0, 2)));
            Assert.That(repeat.Ppe, Is.EqualTo(new SheetImportCounts(0, 0, 4)));
            Assert.That(repeat.ToDelete, Is.Empty);
            Assert.That(await _database.FingerprintAsync(), Is.EqualTo(after), "an unchanged import writes nothing");
        });
    }

    [Test]
    public async Task A_failure_in_the_last_step_rolls_back_deletes_cards_and_compositions()
    {
        await using var host = await SheetImportHost.CreateAsync(_database.Connection);
        await SeedReferenceAsync(host);
        var first = await host.ImportAsync(Body(dryRun: false));
        Assert.That(first.Problems, Is.Empty, Describe(first));

        // Only the database rejects this PPE offer version, after deletes, cards and compositions were saved.
        await _database.ExecuteAsync($"""
            CREATE OR REPLACE FUNCTION catalog.reject_test_offer() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
              IF NEW."ValuesJson"::text LIKE '%{FailingSupplierPrice}%' THEN
                RAISE EXCEPTION 'test offer rejected' USING ERRCODE = '23514', CONSTRAINT = 'CK_test_offer';
              END IF;
              RETURN NEW;
            END $$;
            CREATE TRIGGER reject_test_offer BEFORE INSERT ON catalog."PpePricingOfferVersions"
              FOR EACH ROW EXECUTE FUNCTION catalog.reject_test_offer();
            """);
        try
        {
            var before = await _database.FingerprintAsync();
            var failing = Body(dryRun: false, changeSewing: true, ppePrice: decimal.Parse(FailingSupplierPrice, System.Globalization.CultureInfo.InvariantCulture),
                sewingOnly: false, deleteSecondModel: true);
            var response = await host.PostAsync(failing);
            var text = await response.Content.ReadAsStringAsync();
            Assert.Multiple(async () =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), text);
                Assert.That(text, Does.Contain("CK_test_offer"));
                Assert.That(await _database.FingerprintAsync(), Is.EqualTo(before), "nothing of the failed import is saved");
            });
        }
        finally
        {
            await _database.ExecuteAsync("""
                DROP TRIGGER IF EXISTS reject_test_offer ON catalog."PpePricingOfferVersions";
                DROP FUNCTION IF EXISTS catalog.reject_test_offer();
                """);
        }

        var retried = await host.ImportAsync(Body(dryRun: false, changeSewing: true, ppePrice: 130m, sewingOnly: false, deleteSecondModel: true));
        Assert.That(retried.Applied, Is.True, Describe(retried));
        Assert.That(await _database.ScalarAsync<long>("SELECT count(*) FROM catalog.\"Products\" WHERE \"ModelCode\" = 'MOD-102'"), Is.Zero);
    }

    [Test]
    public async Task Sewing_prices_are_published_automatically_after_an_import_and_after_a_fabric_price_change()
    {
        await using var host = await SheetImportHost.CreateAsync(_database.Connection);
        await SeedReferenceAsync(host);
        var imported = await host.ImportAsync(Body(dryRun: false));
        Assert.That(imported.Problems, Is.Empty, Describe(imported));

        // The import saved the compositions: their prices are published without any button.
        var model = (await SewingPricesAsync(host)).Single(x => x.Articles.Any(a => a.SaleSku == "101"));
        Assert.Multiple(async () =>
        {
            Assert.That(model.Status, Is.EqualTo("Ready"), string.Join(",", model.Blockers));
            Assert.That(model.From, Is.Not.Null);
            Assert.That(model.PricesUpdatedAt, Is.Not.Null);
            Assert.That(model.Articles.Select(x => x.FabricId), Has.All.Not.Null, "each Article is named by its fabric");
            Assert.That(model.Articles.Where(x => x.Active), Has.All.Matches<ArticlePrice>(x => !x.IsStale && x.Published == x.Live));
            Assert.That(model.From!.Retail, Is.EqualTo(model.Articles.Min(x => x.Published!.Retail)));
            Assert.That(await PublicationsAsync(model.ProductId), Is.EqualTo(2));
            Assert.That(await _database.ScalarAsync<string>(
                $"SELECT min(p.\"PublishedBy\") FROM catalog.\"SewingPricePublications\" p JOIN catalog.\"SewingCalculations\" c ON c.\"Id\" = p.\"CalculationId\" WHERE c.\"ProductId\" = {model.ProductId}"),
                Is.EqualTo(SheetImportHost.Actor), "the history names who made the change");
        });

        // A repeat import changes nothing, so nothing new is published.
        var repeat = await host.ImportAsync(Body(dryRun: false));
        Assert.That(repeat.Problems, Is.Empty, Describe(repeat));
        Assert.That(await PublicationsAsync(model.ProductId), Is.EqualTo(2));

        // The fabric gets more expensive: the new price is published at once and the old one stays in history.
        var retail = model.From!.Retail;
        await host.PostOkAsync("api/reference/fabrics/import", new
        {
            dryRun = false, deactivateMissing = false,
            rows = new[] { new { rowNumber = 2, code = "FAB-001", name = "Тканина тест", nameRu = "Ткань тест", supplierName = "Тканини ТОВ",
                composition = "100% бавовна", compositionRu = "100% хлопок", density = "200", price = 250m, priceUpdatedAt = "2026-11-03",
                isActive = true, comment = (string?)null, proposalDescription = (string?)null } }
        });
        model = (await SewingPricesAsync(host)).Single(x => x.ProductId == model.ProductId);
        Assert.Multiple(async () =>
        {
            Assert.That(model.Status, Is.EqualTo("Ready"));
            Assert.That(model.From!.Retail, Is.GreaterThan(retail));
            Assert.That(model.Articles.Where(x => x.Active), Has.All.Matches<ArticlePrice>(x => !x.IsStale));
            Assert.That(await PublicationsAsync(model.ProductId), Is.EqualTo(4));
        });

        // A change without a new price (only the name) publishes nothing.
        await host.PostOkAsync("api/reference/fabrics/import", new
        {
            dryRun = false, deactivateMissing = false,
            rows = new[] { new { rowNumber = 2, code = "FAB-001", name = "Тканина тест нова", nameRu = "Ткань тест", supplierName = "Тканини ТОВ",
                composition = "100% бавовна", compositionRu = "100% хлопок", density = "200", price = 250m, priceUpdatedAt = "2026-11-03",
                isActive = true, comment = (string?)null, proposalDescription = (string?)null } }
        });
        Assert.That(await PublicationsAsync(model.ProductId), Is.EqualTo(4));

        // A new seamstress salary in «Параметри ціноутворення» reaches the sewing settings and republishes every model.
        await host.PostOkAsync("api/reference/pricing-parameters/import", new
        {
            dryRun = false, deleteMissing = false,
            parameters = new[] { new { rowNumber = 2, code = "SALARY_SEAMSTRESS", group = "Тест", name = "SALARY_SEAMSTRESS", value = 19000m,
                unit = "од.", isCalculated = false, updatedOn = "2026-11-03", comment = (string?)null } },
            ppeMarkupRules = new[] { new { rowNumber = 2, name = "Рукавички тест", retailMarkup = .5m, wholesaleMarkup = .36m,
                largeWholesaleMarkup = .3m, priceNote = (string?)null, appliesTo = (string?)null } }
        });
        Assert.That(await PublicationsAsync(model.ProductId), Is.EqualTo(6));

        // A price saved outside the API (a job or a script) waits for the nightly brevierp-sync step.
        await using (var seed = SeedServices())
        {
            await using (var scope = seed.CreateAsyncScope())
            {
                var reference = scope.ServiceProvider.GetRequiredService<ReferenceDbContext>();
                var fabric = await reference.Fabrics.SingleAsync(x => x.Code == "FAB-001");
                fabric.SetPrice(MoneyAmount.From(300m), new DateOnly(2026, 11, 4));
                await reference.SaveChangesAsync();
            }
            Assert.That((await SewingPricesAsync(host)).Single(x => x.ProductId == model.ProductId).Status, Is.EqualTo("Stale"));

            await SyncJob.RunAsync(seed, "sewing-prices");
        }
        model = (await SewingPricesAsync(host)).Single(x => x.ProductId == model.ProductId);
        Assert.Multiple(async () =>
        {
            Assert.That(model.Status, Is.EqualTo("Ready"));
            Assert.That(await PublicationsAsync(model.ProductId), Is.EqualTo(8));
        });

        var ppe = await host.Client.GetFromJsonAsync<List<PriceRow>>("api/v1/products/overview/prices?type=Ppe");
        var ppeId = await _database.ScalarAsync<int>("SELECT p.\"Id\" FROM catalog.\"Products\" p JOIN catalog.\"ProductCatalogProfiles\" c ON c.\"ProductId\" = p.\"Id\" WHERE c.\"SaleSku\" = '501'");
        var noRule = await _database.ScalarAsync<int>("SELECT p.\"Id\" FROM catalog.\"Products\" p JOIN catalog.\"ProductCatalogProfiles\" c ON c.\"ProductId\" = p.\"Id\" WHERE c.\"SaleSku\" = '502'");
        Assert.Multiple(() =>
        {
            Assert.That(ppe!.Single(x => x.ProductId == ppeId).From!.Retail, Is.EqualTo(120m));
            Assert.That(ppe!.Single(x => x.ProductId == noRule).Status, Is.EqualTo("Blocked"));
            Assert.That(ppe!.Single(x => x.ProductId == noRule).Blockers, Is.Not.Empty);
        });
    }

    private sealed record Tiers(decimal Retail, decimal Wholesale, decimal LargeWholesale);
    private sealed record ArticlePrice(long ArticleId, string SaleSku, bool Active, int? FabricId, Tiers? Published, Tiers? Live, bool IsStale);
    private sealed record PriceRow(int ProductId, string Status, Tiers? From, DateTimeOffset? PricesUpdatedAt, List<string> Blockers, List<ArticlePrice> Articles);

    /// <summary>The Host.Seed services of the nightly job, pointed at the test database.</summary>
    private ServiceProvider SeedServices()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = _database.Connection
        }).Build();
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        Host.Seed.SeedHostServicesExtensions.AddSeedHostServices(services, configuration);
        return services.BuildServiceProvider();
    }

    private static async Task<List<PriceRow>> SewingPricesAsync(SheetImportHost host)
        => (await host.Client.GetFromJsonAsync<List<PriceRow>>("api/v1/products/overview/prices?type=Sewing"))!;

    private async Task<long> PublicationsAsync(int productId)
        => await _database.ScalarAsync<long>($"SELECT count(*) FROM catalog.\"SewingPricePublications\" p JOIN catalog.\"SewingCalculations\" c ON c.\"Id\" = p.\"CalculationId\" WHERE c.\"ProductId\" = {productId}");

    private async Task<long> OfferCountAsync(int productId)
        => await _database.ScalarAsync<long>($"SELECT count(*) FROM catalog.\"PpePricingOfferVersions\" WHERE \"ProductId\" = {productId}");

    private async Task<long> LinesAsync(string model, string table)
        => await _database.ScalarAsync<long>($"SELECT count(*) FROM catalog.\"{table}\" l JOIN catalog.\"Products\" p ON p.\"Id\" = l.\"ProductId\" WHERE p.\"ModelCode\" = '{model}'");

    private async Task<(long Reviewed, long All)> ReviewedAsync(string model)
    {
        var reviewed = await _database.ScalarAsync<long>($"""
            SELECT (SELECT count(*) FROM catalog."SewingMaterialLines" l JOIN catalog."Products" p ON p."Id" = l."ProductId" WHERE p."ModelCode" = '{model}' AND l."IsReviewed" AND l."ReviewedBy" IS NOT NULL)
                 + (SELECT count(*) FROM catalog."SewingOperationLines" l JOIN catalog."Products" p ON p."Id" = l."ProductId" WHERE p."ModelCode" = '{model}' AND l."IsReviewed" AND l."ReviewedBy" IS NOT NULL)
            """);
        var all = await LinesAsync(model, "SewingMaterialLines") + await LinesAsync(model, "SewingOperationLines");
        return (reviewed, all);
    }

    private static string Describe(SheetImportReport report)
        => string.Join("; ", report.Problems.Select(x => $"{x.Scope} {x.RowNumber} {x.Code}: {x.Message}"));

    private async Task SeedReferenceAsync(SheetImportHost host)
    {
        await host.SeedSuppliersAsync(["Постачальник ЗІЗ"]);
        await host.PostOkAsync("api/reference/product-sections/import", new
        {
            dryRun = false, deleteMissing = false,
            rows = new object[]
            {
                new { rowNumber = 2, type = "Sewing", name = "Тест одяг", isActive = true, comment = (string?)null },
                new { rowNumber = 3, type = "Ppe", name = "Тест ЗІЗ", isActive = true, comment = (string?)null }
            }
        });
        await host.PostOkAsync("api/reference/fabrics/import", new
        {
            dryRun = false, deactivateMissing = false,
            rows = new[] { new { rowNumber = 2, code = "FAB-001", name = "Тканина тест", nameRu = "Ткань тест", supplierName = "Тканини ТОВ",
                composition = "100% бавовна", compositionRu = "100% хлопок", density = "200", price = 150m, priceUpdatedAt = "2026-08-30",
                isActive = true, comment = (string?)null, proposalDescription = (string?)null } }
        });
        await host.PostOkAsync("api/reference/garment-accessories/import", new
        {
            dryRun = false, deactivateMissing = false,
            rows = new[] { new { rowNumber = 2, code = "FUR-001", name = "Нитки тест", supplierName = "Фурнітура ТОВ", price = 48m,
                unit = "шт.", priceUpdatedAt = "2026-08-30", isActive = true, comment = (string?)null } }
        });
        await host.PostOkAsync("api/reference/garment-part-operations/import", new
        {
            dryRun = false, deleteMissing = false,
            rows = new[]
            {
                new { rowNumber = 2, code = "OP-001", groupName = "Рукав", element = "Рукав", variant = "Прямий", name = "Рукав: прямий", min = 6m,
                    minutesPerProduct = false, normCheckedAt = "2026-08-30", isActive = true, notes = (string?)null },
                new { rowNumber = 3, code = "OP-002", groupName = "Рукав", element = "Манжета", variant = "Пряма", name = "Рукав: манжета", min = 3m,
                    minutesPerProduct = false, normCheckedAt = "2026-08-30", isActive = true, notes = (string?)null }
            }
        });
        await host.PostOkAsync("api/reference/pricing-parameters/import", new
        {
            dryRun = false, deleteMissing = false,
            // The sheet's sewing settings, so models can be priced.
            parameters = new (string Code, decimal Value)[]
                {
                    ("SALARY_SEAMSTRESS", 17500m), ("SEAMSTRESS_BONUS", .08m), ("WORK_DAYS_MONTH", 20.5m), ("MINUTES_SHIFT", 480m),
                    ("ASSEMBLY_RATE", .25m), ("CUTTING_RATE", .2174m), ("MASTER_RATE", .11m), ("SHOP_MANAGER_RATE", .12m),
                    ("SEAMSTRESS_COUNT", 12m), ("MONTHLY_OVERHEAD", 210000m), ("SEWING_PRICE_COEF_1", 1.55m),
                    ("SEWING_PRICE_COEF_2", 1.32m), ("SEWING_PRICE_COEF_3", 1.25m)
                }
                .Select((x, i) => new { rowNumber = i + 2, code = x.Code, group = "Тест", name = x.Code, value = x.Value, unit = "од.",
                    isCalculated = false, updatedOn = "2026-08-30", comment = (string?)null }).ToArray(),
            ppeMarkupRules = new[] { new { rowNumber = 2, name = "Рукавички тест", retailMarkup = .5m, wholesaleMarkup = .36m,
                largeWholesaleMarkup = .3m, priceNote = (string?)null, appliesTo = (string?)null } }
        });
        Assert.That(await _database.ScalarAsync<long>("SELECT count(*) FROM catalog.\"PpePricingRuleVersions\" WHERE \"Code\" = 'Рукавички тест'"), Is.EqualTo(1),
            "the Reference rule reaches Catalog through the pricing settings sync");
    }

    private static object OldSewing() => new
    {
        rowNumber = 2, code = "MOD-900", name = "Стара модель", section = "Тест одяг", season = "Літо", isActive = true,
        isVerified = false, verifiedOn = (string?)null, currentSiteUrl = (string?)null, quoteDescription = (string?)null,
        articles = new[] { new { saleSku = "900", isActive = true } },
        materials = new[] { new { rowNumber = 10, article = (string?)null, kind = "Fabric", code = "FAB-001", quantity = 1m, unit = "Metre", isRequired = true, comment = (string?)null } },
        operations = new[] { new { rowNumber = 20, article = (string?)null, code = "OP-001", quantity = 1m, manualMinutes = (decimal?)null, comment = (string?)null } }
    };

    private static object OldPpe() => Ppe(2, "500", "Рукавички робочі", 100m, "Рукавички тест");

    private static object Ppe(int row, string code, string name, decimal? price, string? rule) => new
    {
        rowNumber = row, code, name, section = "Тест ЗІЗ", unit = "пара", currentSiteUrl = "https://example.test/" + code,
        supplierName = "Постачальник ЗІЗ", supplierUrl = code == "503" ? null : "https://supplier.test/" + code, supplierSku = "S-" + code,
        supplierPrice = price, isActive = true, allowManualPrice = false, isVerified = true, verifiedOn = "01.10.2026",
        ruleCode = rule, quoteDescription = "Опис " + code, insightWholesale = (decimal?)null, insightRetail = (decimal?)null
    };

    private static object Body(bool dryRun, bool changeSewing = false, decimal ppePrice = 120m, bool sewingOnly = false,
        bool deleteSecondModel = false)
    {
        object Material(int row, string? article, string kind, string code, decimal quantity, string unit)
            => new { rowNumber = row, article, kind, code, quantity, unit, isRequired = true, comment = (string?)null };
        object Operation(int row, string code, decimal quantity, decimal? manual, string comment)
            => new { rowNumber = row, article = (string?)null, code, quantity, manualMinutes = manual, comment };

        var first = new
        {
            rowNumber = 2, code = "MOD-101", name = "Куртка тестова", section = "Тест одяг", season = "Зима", isActive = true,
            isVerified = true, verifiedOn = "08.09.2026", currentSiteUrl = "https://example.test/mod-101",
            quoteDescription = changeSewing ? "Змінений опис" : "Тепла куртка",
            articles = new[] { new { saleSku = "101", isActive = true }, new { saleSku = "102", isActive = true } },
            materials = new[]
            {
                Material(10, null, "Fabric", "FAB-001", 2.5m, "Metre"),
                Material(11, "101", "Accessory", "FUR-001", 1m, "Piece"),
                Material(12, "102", "Accessory", "fur-001 ", 2m, "Piece")
            },
            operations = new[]
            {
                Operation(20, "OP-001", 1m, null, "Лівий рукав"),
                Operation(21, "OP-001", 1m, 4.5m, "Правий рукав"),
                Operation(22, "OP-002", 2m, null, "Манжети")
            }
        };
        var second = new
        {
            rowNumber = 3, code = "mod-102", name = "Штани тестові", section = "Тест одяг", season = (string?)null, isActive = false,
            isVerified = false, verifiedOn = (string?)null, currentSiteUrl = (string?)null, quoteDescription = (string?)null,
            articles = new[] { new { saleSku = "103", isActive = true }, new { saleSku = "104", isActive = false } },
            materials = new[] { Material(13, null, "Fabric", "FAB-001", 1.2m, "Metre") },
            operations = new[] { Operation(23, "OP-002", 1m, null, "Пояс") }
        };
        var sewing = deleteSecondModel ? new object[] { first } : new object[] { first, second };
        var ppe = sewingOnly ? Array.Empty<object>() : new[]
        {
            Ppe(2, "500", "Рукавички робочі нові", ppePrice, "Рукавички тест"),
            Ppe(3, "501", "Окуляри захисні", 80m, "Рукавички тест"),
            Ppe(4, "502", "Каска тест", 200m, "Невідоме правило"),
            Ppe(5, "503", "Маска тест", null, "Рукавички тест")
        };
        return new { dryRun, deleteMissing = true, sewing, ppe };
    }
}

/// <summary>A throwaway PostgreSQL database with the Reference and Catalog schemas migrated.</summary>
internal sealed class SheetImportDatabase : IAsyncDisposable
{
    private readonly string _maintenance;
    private readonly string _name;

    private SheetImportDatabase(string maintenance, string name, string connection)
    {
        _maintenance = maintenance;
        _name = name;
        Connection = connection;
    }

    public string Connection { get; }

    public static async Task<SheetImportDatabase> CreateAsync(string prefix)
    {
        var configured = Environment.GetEnvironmentVariable("BREVIERP_CATALOG_TEST_CONNECTION");
        Assert.That(configured, Is.Not.Null.And.Not.Empty,
            "Set BREVIERP_CATALOG_TEST_CONNECTION to a PostgreSQL connection string with CREATE DATABASE permission.");
        var name = $"{prefix}_{Guid.NewGuid():N}";
        var maintenance = new NpgsqlConnectionStringBuilder(configured) { Database = "postgres" }.ConnectionString;
        var connection = new NpgsqlConnectionStringBuilder(configured) { Database = name }.ConnectionString;
        await using (var admin = new NpgsqlConnection(maintenance))
        {
            await admin.OpenAsync();
            await using var create = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", admin);
            await create.ExecuteNonQueryAsync();
        }

        await using (var reference = new ReferenceDbContext(new DbContextOptionsBuilder<ReferenceDbContext>().UseNpgsql(connection).Options))
            await reference.Database.MigrateAsync();
        await using (var catalog = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(connection).Options))
            await catalog.Database.MigrateAsync();
        return new SheetImportDatabase(maintenance, name, connection);
    }

    public async Task<T> ScalarAsync<T>(string sql)
    {
        await using var connection = new NpgsqlConnection(Connection);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return value is null or DBNull ? default! : (T)value;
    }

    public async Task<List<int>> IdsAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(Connection);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        var ids = new List<int>();
        while (await reader.ReadAsync()) ids.Add(reader.GetInt32(0));
        return ids;
    }

    public async Task<List<(int, long)>> PairsAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(Connection);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        var pairs = new List<(int, long)>();
        while (await reader.ReadAsync()) pairs.Add((reader.GetInt32(0), reader.GetInt64(1)));
        return pairs;
    }

    public async Task ExecuteAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(Connection);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>Every row of the tables the sheet import writes, as one hash per table.</summary>
    public async Task<string> FingerprintAsync()
    {
        string[] tables = ["Products", "ProductCatalogProfiles", "PpeProductDetails", "SewingProductDetails", "SewingArticles",
            "SewingMaterialLines", "SewingOperationLines", "PpePricingOfferVersions"];
        var parts = new List<string>();
        foreach (var table in tables)
            parts.Add(table + "=" + await ScalarAsync<string>(
                $"SELECT count(*)::text || ':' || coalesce(md5(string_agg(t::text, '|' ORDER BY t::text)), '') FROM catalog.\"{table}\" t"));
        return string.Join(", ", parts);
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(_maintenance);
        await connection.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_name}\" WITH (FORCE)", connection);
        await drop.ExecuteNonQueryAsync();
    }
}

/// <summary>
/// The API host services and controllers on a test server, against the given database, with a test Admin user.
/// Background services are removed.
/// </summary>
internal sealed class SheetImportHost(WebApplication app, HttpClient client, string connection) : IAsyncDisposable
{
    public const string Actor = "sheet-import-test";
    private const string Scheme = "SheetImportTest";

    public HttpClient Client { get; } = client;

    private static string HostApiSettingsPath()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var path = Path.Combine(dir.FullName, "src", "Bootstrapper", "Host.Api", "appsettings.json");
            if (File.Exists(path)) return path;
        }
        throw new FileNotFoundException("Host.Api appsettings.json was not found above the test output.");
    }

    public static async Task<SheetImportHost> CreateAsync(string connection)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders().AddSimpleConsole();
        // The Host.Api appsettings.json with the database pointed at the test database. It is read from the source
        // tree: the copy in the test output can be Host.Seed's file, which lacks the API sections.
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddJsonFile(HostApiSettingsPath(), optional: false);
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = connection,
            ["ApiAccess:AnonymousEnabled"] = "false",
            ["Logging:LogLevel:Default"] = "Warning"
        });
        builder.Services.AddHostServices(builder.Configuration);
        foreach (var hosted in builder.Services.Where(x => x.ServiceType == typeof(IHostedService)).ToList())
            builder.Services.Remove(hosted);
        builder.Services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, AdminHandler>(Scheme, _ => { });
        builder.Services.PostConfigure<AuthenticationOptions>(options =>
        {
            options.DefaultScheme = Scheme;
            options.DefaultAuthenticateScheme = Scheme;
            options.DefaultChallengeScheme = Scheme;
            options.DefaultForbidScheme = Scheme;
        });

        var app = builder.Build();
        app.UseDeveloperExceptionPage(); // a server error shows its exception in the failed assertion
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        var client = app.GetTestClient();
        client.Timeout = TimeSpan.FromMinutes(10);
        return new SheetImportHost(app, client, connection);
    }

    public Task<HttpResponseMessage> PostAsync(object body) => Client.PostAsJsonAsync("api/v1/products/sheet-import", body);

    public async Task<SheetImportReport> ImportAsync(object body)
    {
        var response = await PostAsync(body);
        var text = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), text);
        return (await response.Content.ReadFromJsonAsync<SheetImportReport>())!;
    }

    public async Task<string> PostOkAsync(string path, object body)
    {
        var response = await Client.PostAsJsonAsync(path, body);
        var text = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"{path}: {text}");
        Assert.That(text, Does.Contain("\"applied\":true"), $"{path}: {text}");
        return text;
    }

    public async Task SeedSuppliersAsync(IEnumerable<string> names)
    {
        await using var db = new ReferenceDbContext(new DbContextOptionsBuilder<ReferenceDbContext>().UseNpgsql(connection).Options);
        var stored = await db.Suppliers.ToListAsync();
        var next = stored.Count == 0 ? 0 : stored.Max(x => x.Id.Value);
        var existing = stored.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var name in names.Where(x => !existing.Contains(x)))
            db.Suppliers.Add(Supplier.Create(SupplierId.From(++next), name));
        await db.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await app.StopAsync();
        await app.DisposeAsync();
    }

    private sealed class AdminHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Actor), new Claim(ClaimTypes.Role, "Admin")],
                Scheme.Name, ClaimTypes.Name, ClaimTypes.Role);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }
    }
}
