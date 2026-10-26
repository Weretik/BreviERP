using SewingPricing.Jobs.Review;

namespace UnitTests;

public sealed class PricingInputReviewTests
{
    [Test]
    public void Numeric_scale_is_not_a_source_difference_including_nested_markups()
    {
        var checks = PricingReviewComparison.Fields(new { Quantity = 1.0m, Markups = new { Retail = .50m } },
            new { Quantity = 1.00m, Markups = new { Retail = .5m } });
        Assert.That(checks.Select(x => x.Matches), Is.All.True);
    }

    [Test]
    public void Nonlocal_review_target_is_rejected_before_reading_source_files()
    {
        Assert.ThrowsAsync<InvalidOperationException>(async () => await PricingInputReviewJob.RunAsync(
            "Host=example.invalid;Database=unused", "missing-settings.json", "missing-ppe.json"));
    }

    [Test]
    public void Changed_or_missing_data_is_explicit_without_overwriting_it()
    {
        var checks = PricingReviewComparison.Fields(new { Quantity = 2m, Unit = "Metre" }, new { Quantity = 3m, Unit = "Piece" });
        Assert.That(checks.Where(x => !x.Matches).Select(x => x.Field), Is.EqualTo(new[] { "Quantity", "Unit" }));
        Assert.That(checks.Single(x => x.Field == "Quantity").Actual, Is.EqualTo("3"));
        Assert.That(PricingReviewComparison.Fields(new { Cost = (decimal?)null }, null).First().Matches, Is.False);
    }
}
