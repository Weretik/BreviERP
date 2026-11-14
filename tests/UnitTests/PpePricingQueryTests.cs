using Ardalis.Result;
using Moq;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Commands.CreatePpeOfferVersion;
using Catalog.Application.Features.Pricing.Commands.CreatePpeRuleVersion;
using Catalog.Application.Features.Pricing.Queries.GetPpeProductPrices;
using Catalog.Application.Features.Pricing.Shared.Ppe.Evaluation;
namespace UnitTests;

public sealed class PpePricingQueryTests
{
    [Test]
    public void Invalid_product_sets_are_rejected_by_the_validator()
    {
        var validator = new GetPpeProductPricesValidator();
        foreach (var ids in new int[][] { [], [0], [1, 1], Enumerable.Range(1, 101).ToArray() })
            Assert.That(validator.Validate(new GetPpeProductPricesQuery(ids)).IsValid, Is.False);
        Assert.That(validator.Validate(new GetPpeProductPricesQuery([1, 2])).IsValid, Is.True);
    }
    [Test]
    public void Imported_source_review_is_separate_from_Brevi_approval()
    {
        var offer = new PpeOfferVersion(Guid.NewGuid(), 42, new("rule", 100, 160, 120, "source", "2026-10-25"), false, null, null, DateTimeOffset.UtcNow);
        var result = PpeProductPricingEvaluator.Evaluate(new(42, true, offer, null));
        Assert.That(result.IsEnrolled, Is.True); Assert.That(result.Calculation.IsCalculated, Is.False);
        Assert.That(result.Calculation.Blockers.Select(x => x.Code), Contains.Item("UnapprovedOffer"));
        Assert.That(PpeProductPricingEvaluator.Evaluate(new(42, false, offer, null)).Calculation.Blockers.Single().Code, Is.EqualTo("InactiveProduct"));
    }
    [Test]
    public void Invalid_amount_precision_or_actor_are_rejected_by_the_validators()
    {
        var values = new PpeOfferValues("rule", 100.0000001m, null, null, "source", "2026-10-25");
        var offer = new CreatePpeOfferVersionValidator().Validate(new CreatePpeOfferVersionCommand(42, null, Guid.NewGuid(), values, "manager"));
        Assert.That(offer.Errors.Select(x => x.PropertyName), Is.EquivalentTo(new[] { "Values" }));
        var rule = new CreatePpeRuleVersionValidator().Validate(new CreatePpeRuleVersionCommand(null, Guid.NewGuid(), new("r", new(.5m, .3m, .2m), "source", "date"), ""));
        Assert.That(rule.Errors.Select(x => x.PropertyName), Is.EquivalentTo(new[] { "Actor" }));
    }
}
