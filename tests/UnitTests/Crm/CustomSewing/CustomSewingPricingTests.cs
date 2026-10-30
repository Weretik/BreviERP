using Catalog.Domain.Pricing.Sewing;

namespace UnitTests.Crm.CustomSewing;

public sealed class CustomSewingPricingTests
{
    private static SewingCalculationSettings Settings => new(1000, 10, 100, 10, .25m,
        .1m, .1m, .1m, .1m, 10000, 2m, 1.5m, 1.25m);

    private static SewingMaterialCostInput Fabric(decimal cost = 100m) =>
        new("Fabric:1", 1, cost, false, true, true, true, true, true, true);

    private static SewingOperationCostInput Operation =>
        new("Operation:1", 1, 10, null, true, true, true);

    [TestCase(100, 152, 310, 240, 205)]
    [TestCase(200, 252, 510, 390, 330)]
    public void Fabric_alternatives_have_independent_costs_and_application_sale_added_once(
        decimal fabric, decimal cost, decimal retail, decimal wholesale, decimal large)
    {
        var result = SewingCustomCalculator.Calculate(Settings, [Fabric(fabric)], [Operation], new(5, 5, 12, 30));
        Assert.Multiple(() =>
        {
            Assert.That(result.IsCalculated, Is.True);
            Assert.That(result.Breakdown!.Seamstress, Is.EqualTo(12.5m));
            Assert.That(result.Breakdown.Overhead, Is.EqualTo(12.5m));
            Assert.That(result.InternalCost, Is.EqualTo(cost));
            Assert.That(result.Prices, Is.EqualTo(new SewingPriceTiers(retail, wholesale, large)));
        });
    }

    [Test]
    public void Application_internal_cost_does_not_replace_or_multiply_approved_sale_amount()
    {
        var first = SewingCustomCalculator.Calculate(Settings, [Fabric()], [Operation], new(5, 5, 12, 30));
        var second = SewingCustomCalculator.Calculate(Settings, [Fabric()], [Operation], new(5, 5, 99, 30));
        Assert.Multiple(() =>
        {
            Assert.That(second.InternalCost - first.InternalCost, Is.EqualTo(87));
            Assert.That(second.Prices, Is.EqualTo(first.Prices));
        });
    }

    [TestCase(-1, 0, 0, 0)]
    [TestCase(0, -1, 0, 0)]
    [TestCase(0, 0, -1, 0)]
    [TestCase(0, 0, 0, -1)]
    [TestCase(0, 0, 0, .001)]
    [TestCase(10000001, 0, 0, 0)]
    public void Invalid_or_unpersistable_charges_block_instead_of_silently_rounding(
        decimal delivery, decimal packaging, decimal cost, decimal sale)
    {
        var result = SewingCustomCalculator.Calculate(Settings, [Fabric()], [Operation], new(delivery, packaging, cost, sale));
        Assert.Multiple(() =>
        {
            Assert.That(result.IsCalculated, Is.False);
            Assert.That(result.Prices, Is.Null);
            Assert.That(result.Blockers.Select(x => x.Code), Does.Contain("InvalidCustomCharge"));
        });
    }

    [Test]
    public void Missing_or_unapproved_composition_has_specific_blockers_and_no_zero_prices()
    {
        var result = SewingCustomCalculator.Calculate(Settings,
            [Fabric() with { UnitCost = null, IsApproved = false, UnitMatches = false }],
            [Operation with { LineKey = "OP-155", StandardMinutes = null }], new(0, 0, 0, 0));
        Assert.Multiple(() =>
        {
            Assert.That(result.Prices, Is.Null);
            Assert.That(result.InternalCost, Is.Null);
            Assert.That(result.Blockers.Select(x => x.Code), Does.Contain("MissingMaterialCost")
                .And.Contain("UnapprovedReference").And.Contain("UnitMismatch").And.Contain("MissingOperationTime"));
        });
    }

    [Test]
    public void Empty_custom_composition_requires_fabric_and_operation()
    {
        var result = SewingCustomCalculator.Calculate(Settings, [], [], new(0, 0, 0, 0));
        Assert.That(result.Blockers.Select(x => x.Code), Does.Contain("MissingFabric").And.Contain("MissingOperations"));
    }

    [Test]
    public void Final_tiers_round_away_from_zero_without_rounding_internal_components()
    {
        var settings = Settings with { MonthlySalary = 1, WorkDays = 1, ShiftMinutes = 1,
            SeamstressCount = 1, AssemblyRate = 0, CuttingRate = 0, MasterRate = 0,
            ManagerRate = 0, BonusRate = 0, MonthlyOverhead = 0,
            RetailMultiplier = 1, WholesaleMultiplier = 1, LargeWholesaleMultiplier = 1 };
        var result = SewingCustomCalculator.Calculate(settings, [Fabric(.005m)],
            [Operation with { StandardMinutes = 1 }], new(0, 0, 0, .01m));
        Assert.Multiple(() =>
        {
            Assert.That(result.InternalCost, Is.EqualTo(1.005m));
            Assert.That(result.Prices, Is.EqualTo(new SewingPriceTiers(1.02m, 1.02m, 1.02m)));
        });
    }

    [Test]
    public void Numeric_overflow_is_a_blocker_instead_of_an_exception_or_partial_price()
    {
        var result = SewingCustomCalculator.Calculate(Settings,
            [Fabric(decimal.MaxValue)], [Operation], new(0, 0, 0, 0));
        Assert.That(result.Blockers.Select(x => x.Code), Does.Contain("NumericOverflow"));
        Assert.That(result.Prices, Is.Null);
    }

    [Test]
    public void Application_aggregates_allow_twenty_valid_line_amounts()
    {
        var result = SewingCustomCalculator.Calculate(Settings, [Fabric()], [Operation],
            new(0, 0, 200000000, 200000000));
        Assert.That(result.IsCalculated, Is.True);
        Assert.That(result.InternalCost, Is.EqualTo(200000130));
        Assert.That(result.Prices!.Retail, Is.EqualTo(200000260));
    }

    [Test]
    public void Additional_custom_cost_overflow_does_not_leak_an_otherwise_valid_base_price()
    {
        var settings = Settings with { RetailMultiplier = 1, WholesaleMultiplier = 1, LargeWholesaleMultiplier = 1 };
        var fabric = Fabric(decimal.MaxValue - 1000000m);
        var original = SewingArticleCalculator.Calculate(settings, [fabric], [Operation]);
        Assert.That(original.IsCalculated, Is.True);
        var result = SewingCustomCalculator.Calculate(settings, [fabric], [Operation], new(10000000, 0, 0, 0));
        Assert.That(result.Blockers.Select(x => x.Code), Does.Contain("NumericOverflow"));
        Assert.That(result.Prices, Is.Null);
    }
}
