using Catalog.Domain.Pricing.Ppe;
namespace UnitTests;

public sealed class PpePricingTests
{
    private static readonly PpeMarkups Rule = new(.5m, .36m, .3m);
    [Test]
    public void Fallback_uses_three_source_fractions_and_sheet_rounding()
    {
        var result = PpeCalculator.Calculate(new(518.64m, true, null, null, false, [Rule]));
        Assert.That(result.Prices, Is.EqualTo(new PpePriceTiers(777.96m, 705.35m, 674.23m)));
        Assert.That(PpeCalculator.Calculate(new(.01m, true, null, null, false, [Rule])).Prices!.Retail, Is.EqualTo(.02m));
    }
    [Test]
    public void Full_external_override_does_not_require_cost_or_rule()
    {
        var result = PpeCalculator.Calculate(new(null, false, 237m, 187.9m, true, []));
        Assert.That(result.Prices, Is.EqualTo(new PpePriceTiers(237m, 212.45m, 187.9m)));
    }
    [Test]
    public void Partial_external_values_only_override_their_corresponding_tiers()
    {
        Assert.That(PpeCalculator.Calculate(new(100, true, 160, null, true, [Rule])).Prices,
            Is.EqualTo(new PpePriceTiers(160, 136, 130)));
        Assert.That(PpeCalculator.Calculate(new(100, true, null, 120, true, [Rule])).Prices,
            Is.EqualTo(new PpePriceTiers(150, 136, 120)));
    }
    [Test]
    public void Unapproved_external_prices_use_approved_fallback_instead()
    {
        Assert.That(PpeCalculator.Calculate(new(100, true, 999, 999, false, [Rule])).Prices,
            Is.EqualTo(new PpePriceTiers(150, 136, 130)));
    }
    [Test]
    public void Missing_ambiguous_and_unapproved_fallback_inputs_return_blockers_without_zero_prices()
    {
        foreach (var input in new PpePricingInput[] {
            new(100, true, null, null, false, []), new(100, true, null, null, false, [Rule, Rule]),
            new(null, true, null, null, false, [Rule]), new(100, false, null, null, false, [Rule]) })
        {
            var result = PpeCalculator.Calculate(input);
            Assert.That(result.IsCalculated, Is.False);
            Assert.That(result.Prices, Is.Null);
            Assert.That(result.Blockers, Is.Not.Empty);
        }
    }
    [Test]
    public void Invalid_negative_amounts_and_numeric_overflow_are_blocked()
    {
        Assert.That(PpeCalculator.Calculate(new(-1, true, null, null, false, [Rule])).IsCalculated, Is.False);
        Assert.That(PpeCalculator.Calculate(new(decimal.MaxValue, true, null, null, false, [Rule])).Blockers.Select(x => x.Code), Contains.Item("NumericOverflow"));
        Assert.That(PpeCalculator.Calculate(new(100, true, null, null, false, [new(-1, .2m, .1m)])).IsCalculated, Is.False);
    }
}
