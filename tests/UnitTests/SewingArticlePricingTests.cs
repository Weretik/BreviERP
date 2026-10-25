using Catalog.Domain.Pricing.Sewing;

namespace UnitTests;

public sealed class SewingArticlePricingTests
{
    private static SewingCalculationSettings Settings => new(17500m, 20.5m, 480m, 12m, .25m,
        .2174m, .11m, .12m, .08m, 210000m, 1.55m, 1.32m, 1.25m);

    [Test]
    public void Manual_only_operations_keep_each_occurrence_and_quantity()
    {
        var result = SewingArticleCalculator.Calculate(Settings,
            [new("Fabric", 1, 10, false, true, true, true, true, true, true)],
            [new("OP-155:1", 1, null, 20m, true, true, true),
             new("OP-155:2", 2, null, 86m, true, true, true)]);

        Assert.That(result.Breakdown!.TotalMinutes, Is.EqualTo(192m));
    }

    [Test]
    public void Currency_rounds_once_and_zero_labor_rates_are_valid()
    {
        var settings = new SewingCalculationSettings(1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1);
        var result = SewingArticleCalculator.Calculate(settings,
            [new("Fabric", 1, .005m, false, true, true, true, true, true, true)],
            [new("Operation", 1, 1, null, true, true, true)]);
        Assert.Multiple(() =>
        {
            Assert.That(result.Breakdown!.FullCost, Is.EqualTo(1.005m));
            Assert.That(result.Prices!.Retail, Is.EqualTo(1.01m));
        });
    }

    [Test]
    public void Inactive_or_unapproved_cost_and_incompatible_unit_are_blockers()
    {
        var result = SewingArticleCalculator.Calculate(Settings,
            [new("Fabric", 1, 10, false, true, true, true, false, false, false)],
            [new("Operation", 1, 1, null, true, true, true)]);
        Assert.That(result.Prices, Is.Null);
        Assert.That(result.Blockers.Select(x => x.Code), Does.Contain("UnitMismatch").And.Contain("UnapprovedReference"));
    }

    [Test]
    public void Settings_reject_percent_magnitudes_in_fraction_fields()
    {
        Assert.Throws<ArgumentException>(() => SewingPricingSettingsVersion.Create(1, DateTimeOffset.UtcNow,
            Settings with { CuttingRate = 21.74m }, new string('A', 64), "{}", true));
    }

    [Test]
    public void Settings_reject_values_that_would_silently_round_in_storage()
    {
        Assert.That(SewingSettingsValidation.Validate(Settings with { CuttingRate = .123456789m })
            .Select(x => x.Code), Does.Contain("InvalidSettingPrecision"));
    }

    [Test]
    public void Settings_return_blockers_for_extreme_negative_values_without_overflowing_validation()
    {
        Assert.That(SewingSettingsValidation.Validate(Settings with { MonthlySalary = decimal.MinValue })
            .Select(x => x.Code), Does.Contain("InvalidSettingPrecision"));
    }

    [TestCase(257.99, 1169.74, 996.17, 943.34)]
    [TestCase(524.39, 1582.66, 1347.81, 1276.34)]
    public void Current_source_fixture_uses_multiplicative_roles_and_three_tiers(
        decimal materialCost, decimal retail, decimal wholesale, decimal largeWholesale)
    {
        var result = SewingArticleCalculator.Calculate(Settings,
            [new("Fabric", 1m, materialCost, false, true, true, true, true, true, true),
             new("OtherCost", 20m, 1m, true, false, true, true, true, true, true)],
            [new("Operations", 1m, 84.84m, null, true, true, true)]);

        Assert.That(result.IsCalculated, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Breakdown!.Cutting, Is.EqualTo(result.Breakdown.Seamstress * .2174m));
            Assert.That(result.Prices, Is.EqualTo(new SewingPriceTiers(retail, wholesale, largeWholesale)));
        });
    }

    [TestCase(9, 10)]
    [TestCase(10, 20)]
    [TestCase(40, 20)]
    [TestCase(41, 30)]
    public void Tier_boundaries_are_unambiguous(int quantity, decimal expected)
        => Assert.That(new SewingPriceTiers(10m, 20m, 30m).ForQuantity(quantity), Is.EqualTo(expected));

    [Test]
    public void Missing_divisor_and_unreviewed_inputs_block_without_zero_price()
    {
        var result = SewingArticleCalculator.Calculate(Settings with { WorkDays = 0 },
            [new("Fabric", 1, null, false, true, true, false, true, true, true)],
            [new("Operation", 1, null, null, false, true, true)]);
        Assert.Multiple(() =>
        {
            Assert.That(result.Prices, Is.Null);
            Assert.That(result.Blockers.Select(x => x.Code), Does.Contain("InvalidSchedule"));
            Assert.That(result.Blockers.Select(x => x.Code), Does.Contain("UnreviewedMaterial"));
            Assert.That(result.Blockers.Select(x => x.Code), Does.Contain("MissingOperationTime"));
        });
    }
}

