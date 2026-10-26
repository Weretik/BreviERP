using System.Text.Json;
using Catalog.Domain.Products.Entities;
using Catalog.Infrastructure.Pricing.Ppe.Records;
using CatalogImport.PpePricing.Bootstrap;
using IntegrationTests.Pricing.Sewing;
using Microsoft.EntityFrameworkCore;
using SewingPricing.Jobs.Bootstrap;
using SewingPricing.Jobs.Review;

namespace IntegrationTests.Pricing;

[NonParallelizable]
public sealed class PricingInputReviewTests : SewingPricingDatabaseFixture
{
    [Test]
    public async Task Review_matches_source_without_approving_and_detects_changed_or_missing_lines()
    {
        await PricingBootstrapJob.RunAsync(_connection, _pricingSource);
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"review-ppe-{Guid.NewGuid():N}.json");
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
            var before = await PricingInputReviewJob.RunAsync(_connection, _pricingSource, path);
            Assert.That(before.DifferentSubjects, Is.Zero);
            Assert.That(before.Subjects.Select(x => x.Group), Does.Contain("MaterialLine").And.Contain("OperationLine").And.Contain("PpeOffer").And.Contain("ReferenceInput"));
            Assert.That(before.CanApply, Is.False);
            await using var db = Catalog();
            Assert.That(await db.Set<PpeOfferVersionRecord>().AnyAsync(x => x.ApprovedBy != null), Is.False);
            Assert.That(await db.Set<SewingMaterialLine>().AnyAsync(x => x.IsReviewed), Is.False);
            var lineId = (await db.Set<SewingMaterialLine>().FirstAsync()).Id.Value;
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE catalog.\"SewingMaterialLines\" SET \"Quantity\" = \"Quantity\" + 1 WHERE \"Id\" = {lineId}");
            var changed = await PricingInputReviewJob.RunAsync(_connection, _pricingSource, path);
            Assert.That(changed.Subjects.Where(x => x.Group == "MaterialLine").Any(x => x.Checks.Any(c => c.Field == "Quantity" && !c.Matches)), Is.True);
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM catalog.\"SewingMaterialLines\" WHERE \"Id\" = {lineId}");
            var missing = await PricingInputReviewJob.RunAsync(_connection, _pricingSource, path);
            Assert.That(missing.Subjects.Where(x => x.Group == "MaterialLine").Any(x => x.Checks.Any(c => c.Field == "Exists" && !c.Matches)), Is.True);
            Assert.That(missing.Subjects.Single(x => x.Group == "PpeOffer").MatchesSource, Is.True);
        }
        finally { File.Delete(path); }
    }
}
