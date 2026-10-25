using System.Text.Json;
using Catalog.Domain.Pricing.Ppe;
using CatalogImport.PpePricing.Verification;
using CatalogImport.Source;

namespace UnitTests;

public sealed class PpePricingParityTests
{
    [Test]
    public void Diagnostic_compares_every_tier_and_keeps_expected_actual_and_difference()
    {
        var values = PpePricingParityComparer.Compare(Row(150, 136, 130), new PpePriceTiers(150, 136, 130));
        Assert.That(values.Select(x => x.Status), Is.All.EqualTo("Matched"));
        Assert.That(values[1].SourceCell, Is.EqualTo("Прайс на сайт!I2"));
        Assert.That(values[1].Difference, Is.Zero);
        var changed = PpePricingParityComparer.Compare(Row(150, 137, 130), new PpePriceTiers(150, 136, 130));
        Assert.That(changed.Single(x => x.Status == "Different").Difference, Is.EqualTo(-1));
    }

    [Test]
    public void Missing_source_is_distinct_from_invalid_source_and_missing_calculation()
    {
        Assert.That(PpePricingParityComparer.Compare(Row(null, null, null), null).Select(x => x.Status), Is.All.EqualTo("MissingSourceValue"));
        Assert.That(PpePricingParityComparer.Compare(Row("#N/A", 136, 130), new(150, 136, 130))[0].Status, Is.EqualTo("InvalidSourceValue"));
        Assert.That(PpePricingParityComparer.Compare(Row(150, 136, 130), null).Select(x => x.Status), Is.All.EqualTo("MissingCalculation"));
    }

    private static WorkbookRow Row(object? retail, object? wholesale, object? large)
        => new("Прайс на сайт", 2, JsonSerializer.Deserialize<JsonElement[]>(JsonSerializer.Serialize(
            new object?[] { "724", null, null, null, null, "ЗІЗ", null, retail, wholesale, large }))!);
}
