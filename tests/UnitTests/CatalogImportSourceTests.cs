using System.Text.Json;
using CatalogImport.Source;
using CatalogImport.Planning;

namespace UnitTests;

public sealed class CatalogImportSourceTests
{
    [Test]
    public async Task Manual_operation_without_reference_and_unused_placeholder_are_explicitly_retained()
    {
        var (snapshot, _, _) = await WorkbookSnapshot.LoadAsync(
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "catalog-import.json"),
            CancellationToken.None);
        snapshot.Sheets["Трудовитрати виробів"][1] = JsonSerializer.Deserialize<JsonElement[]>(
            """["Model","MOD-025",null,null,null,null,null,1,4,null,null,"Source manual operation"]""")!;
        snapshot.Sheets["Тканини"] = [.. snapshot.Sheets["Тканини"],
            JsonSerializer.Deserialize<JsonElement[]>("""["FAB-EMPTY",null,null,null,null,null,null,false]""")!];

        var plan = CatalogImportPlanner.Build(snapshot);

        Assert.Multiple(() =>
        {
            Assert.That(plan.Operations, Has.Count.EqualTo(2));
            Assert.That(CatalogSourceIdentity.OperationKey(plan.Operations[0]), Does.StartWith("MANUAL:"));
            Assert.That(CatalogSourceReferences.Fabrics(snapshot), Has.Count.EqualTo(1));
            Assert.That(plan.Warnings, Has.Some.Contains("placeholder"));
        });
    }

    [Test]
    public async Task Planner_retains_each_operation_and_reports_missing_ppe_cost()
    {
        var (snapshot, _, _) = await WorkbookSnapshot.LoadAsync(
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "catalog-import.json"),
            CancellationToken.None);

        var plan = CatalogImportPlanner.Build(snapshot);

        Assert.Multiple(() =>
        {
            Assert.That(plan.Models, Has.Count.EqualTo(1));
            Assert.That(plan.Articles, Has.Count.EqualTo(2));
            Assert.That(plan.Operations, Has.Count.EqualTo(2));
            Assert.That(plan.Warnings, Has.Some.Contains("no supplier price"));
        });
        snapshot.Sheets["Комплектація виробів"][1][6] = JsonSerializer.SerializeToElement("FAB-MISSING");
        Assert.Throws<InvalidDataException>(() => CatalogImportPlanner.Build(snapshot));
    }

    [Test]
    public void Ambiguous_localized_numeric_text_is_rejected_in_an_unformatted_snapshot()
    {
        var row = new WorkbookRow("Source", 2, [JsonSerializer.SerializeToElement("1,5")]);

        Assert.Throws<InvalidDataException>(() => row.Decimal(0));
    }

    [Test]
    public void Numeric_quantities_and_null_values_are_preserved()
    {
        var row = new WorkbookRow("Source", 2,
            [JsonSerializer.SerializeToElement(1.5m), JsonSerializer.SerializeToElement<object?>(null)]);

        Assert.Multiple(() =>
        {
            Assert.That(row.Decimal(0), Is.EqualTo(1.5m));
            Assert.That(row.Decimal(1), Is.Null);
        });
    }
}
