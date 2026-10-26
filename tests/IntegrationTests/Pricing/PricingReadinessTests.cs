using System.Text.Json;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Infrastructure.Pricing.Ppe.Records;
using CatalogImport.Source;
using IntegrationTests.Pricing.Sewing;
using Microsoft.EntityFrameworkCore;
using SewingPricing.Jobs.Readiness;

namespace IntegrationTests.Pricing;

[NonParallelizable]
public sealed class PricingReadinessTests : SewingPricingDatabaseFixture
{
    [Test]
    public async Task Full_inventory_preserves_draft_blockers_and_does_not_write_business_data()
    {
        var source = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "catalog-import.json");
        var snapshot = (await WorkbookSnapshot.LoadAsync(source, CancellationToken.None)).Snapshot;
        await using var db = Catalog();
        var product = await db.Products.FirstAsync(x => x.Type == ProductType.Ppe);
        var offer = new PpeOfferVersionRecord
        {
            PublicId = Guid.NewGuid(),
            RequestId = Guid.NewGuid(),
            RequestHash = "readiness-offer",
            ProductId = product.Id,
            CreatedAt = EffectiveFrom,
            ValuesJson = JsonSerializer.Serialize(new PpeOfferValues("ReadinessRule", 100m, null, null, "test source", "2026-10-25", true))
        };
        var rule = new PpeRuleVersionRecord
        {
            PublicId = Guid.NewGuid(),
            RequestId = Guid.NewGuid(),
            RequestHash = "readiness-rule",
            Code = "ReadinessRule",
            CreatedAt = EffectiveFrom,
            ValuesJson = JsonSerializer.Serialize(new PpeRuleValues("ReadinessRule", new(.5m, .36m, .3m), "test source", "2026-10-25"))
        };
        db.AddRange(offer, rule);
        await db.SaveChangesAsync();
        var offerCount = await db.Set<PpeOfferVersionRecord>().CountAsync();
        var ruleCount = await db.Set<PpeRuleVersionRecord>().CountAsync();
        var report = await PricingReadinessJob.RunAsync(_connection, EffectiveFrom);
        Assert.That(report.Items.Count, Is.EqualTo(snapshot.Rows("Прайс на сайт").Count));
        Assert.That(report.Items.Select(x => x.Kind), Does.Contain("Ppe").And.Contain("Sewing"));
        Assert.That(report.CalculationReadyCount, Is.Zero);
        Assert.That(report.CanActivate, Is.False);
        Assert.That(report.Items.Select(x => x.PublicationId), Is.All.Null);
        var ppe = report.Items.Single(x => x.Kind == "Ppe");
        Assert.That(ppe.OfferVersionId, Is.EqualTo(offer.PublicId));
        Assert.That(ppe.RuleVersionId, Is.EqualTo(rule.PublicId));
        Assert.That(ppe.OfferApproved, Is.False);
        Assert.That(ppe.RuleApproved, Is.False);
        Assert.That(ppe.Blockers, Is.Not.Empty);
        await db.Entry(offer).ReloadAsync();
        await db.Entry(rule).ReloadAsync();
        Assert.That(offer.ApprovedBy, Is.Null);
        Assert.That(rule.ApprovedBy, Is.Null);
        Assert.That(offer.Values().SupplierCost, Is.EqualTo(100m));
        Assert.That(await db.Set<PpeOfferVersionRecord>().CountAsync(), Is.EqualTo(offerCount));
        Assert.That(await db.Set<PpeRuleVersionRecord>().CountAsync(), Is.EqualTo(ruleCount));
        Assert.That(await db.Database.SqlQueryRaw<long>("SELECT count(*) AS \"Value\" FROM catalog.\"SewingCalculations\"").SingleAsync(), Is.Zero);
        Assert.That(await db.Database.SqlQueryRaw<long>("SELECT count(*) AS \"Value\" FROM catalog.\"SewingPricePublications\"").SingleAsync(), Is.Zero);
    }

    [Test]
    public async Task Missing_article_is_retained_in_inventory_as_a_blocker()
    {
        await using var db = Catalog();
        var sku = await db.Set<SewingArticle>().Select(x => x.SaleSku).FirstAsync();
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE catalog.\"SewingArticles\" SET \"SaleSku\" = {"readiness-hidden"} WHERE \"SaleSku\" = {sku}");
        try
        {
            var report = await PricingReadinessJob.RunAsync(_connection, EffectiveFrom);
            var missing = report.Items.Single(x => x.SaleSku == sku);
            Assert.That(missing.ArticleId, Is.Null);
            Assert.That(missing.CalculationReady, Is.False);
            Assert.That(missing.Blockers.Select(x => x.Code), Contains.Item("MissingArticle"));
        }
        finally
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE catalog.\"SewingArticles\" SET \"SaleSku\" = {sku} WHERE \"SaleSku\" = {"readiness-hidden"}");
        }
    }

}
