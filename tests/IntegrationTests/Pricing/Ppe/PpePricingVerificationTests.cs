using Catalog.Infrastructure.Pricing.Ppe.Records;
using CatalogImport.PpePricing.Verification;
using IntegrationTests.Pricing.Sewing;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Pricing.Ppe;

[NonParallelizable]
public sealed class PpePricingVerificationTests : SewingPricingDatabaseFixture
{
    [Test]
    public async Task Diagnostic_reports_missing_inputs_without_creating_or_approving_any_version()
    {
        var report = await PpePricingVerificationJob.RunAsync(_connection);
        Assert.That(report.Products, Has.Count.EqualTo(1));
        Assert.That(report.Products.Single().CanPublish, Is.False);
        Assert.That(report.Products.Single().StrictBlockers.Select(x => x.Code), Contains.Item("MissingOffer"));
        Assert.That(report.Products.Single().Comparisons.Select(x => x.Status), Is.All.EqualTo("MissingSourceValue"));
        await using var db = Catalog();
        Assert.That(await db.Set<PpeOfferVersionRecord>().CountAsync(), Is.Zero);
        Assert.That(await db.Set<PpeRuleVersionRecord>().CountAsync(), Is.Zero);
    }
}
