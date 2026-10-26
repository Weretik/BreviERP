using System.Text.Json;
using Catalog.Application.Contracts.Pricing;
using Catalog.Domain.Products.Entities;
using Catalog.Infrastructure.DependencyInjection;
using Catalog.Infrastructure.Pricing.Ppe.Records;
using CatalogImport.PpePricing.Bootstrap;
using CatalogImport.Source;
using IntegrationTests.Pricing.Sewing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reference.Infrastructure.DependencyInjection;
using SewingPricing.Jobs.Activation;
using SewingPricing.Jobs.Bootstrap;
using SewingPricing.Jobs.Review;

namespace IntegrationTests.Pricing;

[NonParallelizable]
public sealed class PricingActivationTests : SewingPricingDatabaseFixture
{
    private string _catalogueSource = string.Empty;

    protected override string CatalogueSourcePath()
    {
        var original = base.CatalogueSourcePath();
        var snapshot = JsonSerializer.Deserialize<WorkbookSnapshot>(File.ReadAllText(original), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        var rows = snapshot.Sheets["Прайс на сайт"];
        for (var index = 1; index < rows.Length; index++)
        {
            var cells = rows[index].Concat(Enumerable.Repeat(JsonSerializer.SerializeToElement((object?)null), Math.Max(0, 24 - rows[index].Length))).ToArray();
            var offset = cells[5].GetString() == "ЗІЗ" ? 7 : 21;
            for (var column = offset; column < offset + 3; column++) cells[column] = JsonSerializer.SerializeToElement(0m);
            rows[index] = cells;
        }
        _catalogueSource = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"activation-catalogue-{Guid.NewGuid():N}.json");
        File.WriteAllText(_catalogueSource, JsonSerializer.Serialize(snapshot, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        return _catalogueSource;
    }

    [OneTimeTearDown]
    public void RemoveCatalogueFixture()
    {
        if (File.Exists(_catalogueSource)) File.Delete(_catalogueSource);
    }

    [Test]
    public async Task Input_based_activation_preserves_exclusions_rejects_stale_data_and_replays_without_duplicate_history()
    {
        await PricingBootstrapJob.RunAsync(_connection, _pricingSource);
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"activation-ppe-{Guid.NewGuid():N}.json");
        var row = new object?[18];
        row[0] = "724"; row[7] = 100m; row[11] = true; row[12] = "2026-10-25"; row[13] = "Rule";
        try
        {
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(new
            {
                version = 1,
                spreadsheetId = "test-workbook",
                capturedAt = "2026-10-25",
                rules = new object?[][] { ["Code", "Retail", "Wholesale", "Large"], ["Rule", .5m, .36m, .3m] },
                products = new object?[][] { ["Sku"], row }
            }));
            await PpePricingBootstrapJob.RunAsync(_connection, path);
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            { ["ConnectionStrings:Default"] = _connection }).Build();
            await using var services = new ServiceCollection().AddLogging().AddCatalogDbContextServices(configuration)
                .AddReferenceDbContextServices(configuration).AddPricingActivationServices().BuildServiceProvider();
            // Complete isolated source has zero cached outputs; activation must still calculate from actual inputs.
            var plan = await PricingActivationPlanner.BuildAsync(services, _connection, "1", []);
            await using var db = Catalog();
            var initialRules = await db.Set<PpeRuleVersionRecord>().CountAsync();
            Assert.ThrowsAsync<IOException>(async () => await PricingActivationJob.RunAsync(services, _connection, plan,
                _ => throw new IOException("Progress path is unavailable.")));
            Assert.That(await db.Set<PpeRuleVersionRecord>().CountAsync(), Is.EqualTo(initialRules), "Reject an unusable journal before any confirmation.");
            var lineId = (await db.Set<SewingMaterialLine>().FirstAsync()).Id.Value;
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE catalog.\"SewingMaterialLines\" SET \"Quantity\" = \"Quantity\" + 1 WHERE \"Id\" = {lineId}");
            Assert.ThrowsAsync<InvalidOperationException>(async () => await PricingActivationJob.RunAsync(services, _connection, plan, _ => Task.CompletedTask));
            Assert.That(await db.Set<PpeRuleVersionRecord>().CountAsync(), Is.EqualTo(initialRules));
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE catalog.\"SewingMaterialLines\" SET \"Quantity\" = \"Quantity\" - 1 WHERE \"Id\" = {lineId}");
            var excluded = plan with { Id = Guid.NewGuid(), Rules = [], Offers = [], ExcludedSkus = ["724", "931"] };
            var first = await PricingActivationJob.RunAsync(services, _connection, excluded, _ => Task.CompletedTask);
            Assert.That(first.Status, Is.EqualTo("Complete"));
            Assert.That(await db.Set<PpeOfferVersionRecord>().AnyAsync(x => x.ApprovedBy != null), Is.False);
            var activated = await PricingActivationJob.RunAsync(services, _connection, plan, _ => Task.CompletedTask);
            Assert.That(activated.Status, Is.EqualTo("Complete"));
            var replay = await PricingActivationJob.RunAsync(services, _connection, plan, _ => Task.CompletedTask);
            Assert.That(replay.Steps.Where(x => x.PublicationId.HasValue).Select(x => x.PublicationId),
                Is.EqualTo(first.Steps.Where(x => x.PublicationId.HasValue).Select(x => x.PublicationId)));
            Assert.That(await db.Set<PpeOfferVersionRecord>().CountAsync(), Is.EqualTo(2));
            Assert.That(await db.Set<PpeRuleVersionRecord>().CountAsync(), Is.EqualTo(2));
            Assert.That(await db.Database.SqlQueryRaw<long>("SELECT count(*) AS \"Value\" FROM catalog.\"SewingCalculations\"").SingleAsync(), Is.EqualTo(2));
            Assert.That(await db.Database.SqlQueryRaw<long>("SELECT count(*) AS \"Value\" FROM catalog.\"SewingPricePublications\"").SingleAsync(), Is.EqualTo(2));
            var verified = await PricingActivationVerification.VerifyAsync(services, plan);
            Assert.That(verified.PublishedSewingArticles, Is.EqualTo(2));
            Assert.That(verified.CalculablePpeProducts, Is.EqualTo(1));
            var review = await PricingInputReviewJob.RunAsync(_connection, _pricingSource, path);
            Assert.That(review.DifferentSubjects, Is.Zero, "Legitimate new composition line IDs preserve source equivalence.");
            Assert.That(db.Database.HasPendingModelChanges(), Is.False);
        }
        finally { File.Delete(path); }
    }
}
