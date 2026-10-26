using System.Text.Json;
using CatalogImport.Source;
using SewingPricing.Jobs.Readiness;

namespace UnitTests;

public sealed class PricingReadinessTests
{
    [Test]
    public void Missing_source_outputs_keep_each_cell_instead_of_becoming_zero()
    {
        var row = Row("ЗІЗ", null, null, null);
        Assert.That(PricingReadinessSourceCheck.Inspect(row), Is.EqualTo(new[]
        {
            "MissingSourceValue:Прайс на сайт!H2", "MissingSourceValue:Прайс на сайт!I2", "MissingSourceValue:Прайс на сайт!J2"
        }));
    }

    [Test]
    public void Invalid_source_value_is_separate_from_missing_and_numeric_zero()
    {
        Assert.That(PricingReadinessSourceCheck.Inspect(Row("ЗІЗ", "#N/A", null, 0)), Is.EqualTo(new[]
        {
            "InvalidSourceValue:Прайс на сайт!H2", "MissingSourceValue:Прайс на сайт!I2"
        }));
    }

    [Test]
    public void Sewing_issues_use_sewing_output_cells()
    {
        Assert.That(PricingReadinessSourceCheck.Inspect(Row("Швейна продукція", null, 10, 20)),
            Is.EqualTo(new[] { "MissingSourceValue:Прайс на сайт!V2" }));
    }

    [Test]
    public void Calculation_readiness_publication_and_source_review_are_independent()
    {
        var ready = new PricingReadinessItem("246", "Sewing", 1, 1, true, [], []);
        Assert.That(ready.NextAction, Is.EqualTo("SaveAndPublish"));
        Assert.That((ready with { PublicationId = Guid.NewGuid() }).NextAction, Is.EqualTo("ReviewCurrentPublication"));
        Assert.That((ready with { CalculationReady = false, PublicationId = Guid.NewGuid() }).NextAction, Is.EqualTo("ResolveBlockers"));
        Assert.That((ready with { SourceIssues = ["MissingSourceValue:cell"] }).NextAction, Is.EqualTo("ReviewSourceData"));
        Assert.That((ready with { Kind = "Ppe" }).NextAction, Is.EqualTo("UseCurrentPricing"));
    }

    [Test]
    public void Nonlocal_target_is_rejected_before_connecting()
    {
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await PricingReadinessJob.RunAsync("Host=example.invalid;Database=not-used", DateTimeOffset.UtcNow));
    }

    private static WorkbookRow Row(string kind, object? first, object? second, object? third)
    {
        var cells = new object?[24];
        cells[0] = "724";
        cells[5] = kind;
        var offset = kind == "ЗІЗ" ? 7 : 21;
        cells[offset] = first;
        cells[offset + 1] = second;
        cells[offset + 2] = third;
        return new("Прайс на сайт", 2, JsonSerializer.SerializeToElement(cells).EnumerateArray().ToArray());
    }
}
