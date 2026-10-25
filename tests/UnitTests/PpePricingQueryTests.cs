using Ardalis.Result;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Products;
using Catalog.Application.Features.Pricing.Ppe.Evaluation;
using Catalog.Application.Features.Pricing.Ppe.Rules.CreateVersion;
using Catalog.Application.Features.Pricing.Ppe.Offers.CreateVersion;
using Moq;
namespace UnitTests;

public sealed class PpePricingQueryTests
{
    [Test]
    public async Task Invalid_product_sets_never_read_the_database()
    {
        var reader = new Mock<IPpePricingInputReader>(); var handler = new GetPpeProductPricesQueryHandler(reader.Object);
        foreach (var ids in new int[][] { [], [0], [1, 1], Enumerable.Range(1, 101).ToArray() })
            Assert.That((await handler.Handle(new(ids), CancellationToken.None)).Status, Is.EqualTo(ResultStatus.Invalid));
        reader.VerifyNoOtherCalls();
    }
    [Test]
    public void Imported_source_review_is_separate_from_Brevi_approval()
    {
        var offer = new PpeOfferVersion(Guid.NewGuid(), 42, new("rule", 100, 160, 120, "source", "2026-10-25", true), false, null, null, DateTimeOffset.UtcNow);
        var result = PpeProductPricingEvaluator.Evaluate(new(42, true, offer, null));
        Assert.That(result.IsEnrolled, Is.True); Assert.That(result.Calculation.IsCalculated, Is.False);
        Assert.That(result.Calculation.Blockers.Select(x => x.Code), Contains.Item("UnapprovedOffer"));
        Assert.That(PpeProductPricingEvaluator.Evaluate(new(42, false, offer, null)).Calculation.Blockers.Single().Code, Is.EqualTo("InactiveProduct"));
    }
    [Test]
    public async Task Invalid_amount_precision_or_actor_cannot_reach_version_writers()
    {
        var writer = new Mock<IPpeOfferVersionWriter>();
        var handler = new CreatePpeOfferVersionCommandHandler(writer.Object);
        var values = new PpeOfferValues("rule", 100.0000001m, null, null, "source", "2026-10-25", true);
        Assert.That((await handler.Handle(new(42, null, Guid.NewGuid(), values, "manager"), CancellationToken.None)).Status, Is.EqualTo(ResultStatus.Invalid));
        writer.VerifyNoOtherCalls();
        var rules = new Mock<IPpeRuleVersionWriter>();
        Assert.That((await new CreatePpeRuleVersionCommandHandler(rules.Object).Handle(new(null, Guid.NewGuid(), new("r", new(.5m, .3m, .2m), "source", "date"), ""), CancellationToken.None)).Status, Is.EqualTo(ResultStatus.Invalid));
        rules.VerifyNoOtherCalls();
    }
}
