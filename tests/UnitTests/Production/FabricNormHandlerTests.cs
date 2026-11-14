using Ardalis.Result;
using Production.Application.Abstractions;
using Production.Application.Features.FabricNorms.Commands.DeleteFabricNorm;
using Production.Application.Features.FabricNorms.Commands.ImportFabricNorms;
using Production.Application.Features.FabricNorms.Commands.SaveFabricNorm;
using Production.Application.Features.FabricNorms.Queries.GetFabricNorms;
using Production.Application.Features.FabricNorms.Shared;

namespace UnitTests.Production;

public sealed class FabricNormHandlerTests
{
    private static readonly FabricNormInput TwoColors = new(1.25m, 0.9m, 0.4m, null, null, null);

    [Test]
    public async Task Norm_is_saved_for_a_catalog_model_and_listed_with_totals()
    {
        using var workshop = new ProductionTestHarness();
        workshop.Models.Models.AddRange([new("MOD-025", "Куртка", true, 12, "Зима"), new("MOD-030", "Жилет", true)]);

        var saved = await new SaveFabricNormHandler(workshop.Db, workshop.Models, workshop.Time)
            .Handle(new("mod-025", TwoColors, 7), CancellationToken.None);
        var rows = (await new GetFabricNormsHandler(workshop.Db, workshop.Models)
            .Handle(new GetFabricNormsQuery(), CancellationToken.None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(saved.IsSuccess, Is.True);
            Assert.That(rows.Select(x => x.ModelCode), Is.EqualTo(new[] { "MOD-025", "MOD-030" }));
            Assert.That(rows[0].TwoColorsTotal, Is.EqualTo(1.3m));
            Assert.That(rows[0].Problem, Is.Null);
            Assert.That((rows[0].ProductId, rows[0].Season), Is.EqualTo((12, "Зима")), "the card link and season come from Catalog");
            Assert.That((saved.Value.ProductId, saved.Value.Season), Is.EqualTo((12, "Зима")));
            Assert.That(rows[1].Norm, Is.Null, "a model without a norm is listed with an empty norm");
        });
    }

    [Test]
    public async Task Norm_for_an_unknown_model_is_not_saved()
    {
        using var workshop = new ProductionTestHarness();

        var result = await new SaveFabricNormHandler(workshop.Db, workshop.Models, workshop.Time)
            .Handle(new("MOD-999", TwoColors, 7), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task Norm_of_a_model_removed_from_catalog_is_shown_with_its_problem()
    {
        using var workshop = new ProductionTestHarness();
        workshop.Models.Models.Add(new("MOD-025", "Куртка", true));
        await new SaveFabricNormHandler(workshop.Db, workshop.Models, workshop.Time)
            .Handle(new("MOD-025", TwoColors, 7), CancellationToken.None);
        workshop.Models.Models.Clear();

        var rows = (await new GetFabricNormsHandler(workshop.Db, workshop.Models)
            .Handle(new GetFabricNormsQuery(), CancellationToken.None)).Value;

        Assert.That(rows.Single().Problem, Is.Not.Null);
    }

    [Test]
    public async Task Import_with_a_wrong_row_saves_nothing_and_names_the_row()
    {
        using var workshop = new ProductionTestHarness();
        workshop.Models.Models.Add(new("MOD-025", "Куртка", true));
        var handler = new ImportFabricNormsHandler(workshop.Db, workshop.Models, workshop.Time);

        var result = await handler.Handle(new([new("MOD-025", TwoColors), new("MOD-404", TwoColors)], false, 7), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Errors, Is.EqualTo(1));
            Assert.That(result.Value.Lines[1].ModelCode, Is.EqualTo("MOD-404"));
            Assert.That(workshop.Db.FabricNorms.Count(), Is.Zero);
        });
    }

    [Test]
    public async Task Import_creates_then_reports_unchanged_rows()
    {
        using var workshop = new ProductionTestHarness();
        workshop.Models.Models.Add(new("MOD-025", "Куртка", true));
        var handler = new ImportFabricNormsHandler(workshop.Db, workshop.Models, workshop.Time);

        var first = await handler.Handle(new([new("MOD-025", TwoColors)], false, 7), CancellationToken.None);
        var second = await handler.Handle(new([new("MOD-025", TwoColors)], false, 7), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(first.Value.Created, Is.EqualTo(1));
            Assert.That(second.Value.Unchanged, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Delete_removes_the_norm()
    {
        using var workshop = new ProductionTestHarness();
        workshop.Models.Models.Add(new("MOD-025", "Куртка", true));
        await new SaveFabricNormHandler(workshop.Db, workshop.Models, workshop.Time)
            .Handle(new("MOD-025", TwoColors, 7), CancellationToken.None);

        var deleted = await new DeleteFabricNormHandler(workshop.Db).Handle(new("MOD-025"), CancellationToken.None);
        var missing = await new DeleteFabricNormHandler(workshop.Db).Handle(new("MOD-025"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(deleted.IsSuccess, Is.True);
            Assert.That(missing.Status, Is.EqualTo(ResultStatus.NotFound));
        });
    }

    [Test]
    public async Task Validator_rejects_an_additional_fabric_without_the_main_one_and_an_empty_norm()
    {
        var validator = new SaveFabricNormValidator();

        var noMain = await validator.ValidateAsync(new SaveFabricNormCommand("MOD-025", new(null, null, 0.4m, null, null, null), 7));
        var empty = await validator.ValidateAsync(new SaveFabricNormCommand("MOD-025", new(null, null, null, null, null, null), 7));
        var negative = await validator.ValidateAsync(new SaveFabricNormCommand("MOD-025", new(-1m, null, null, null, null, null), 7));

        Assert.Multiple(() =>
        {
            Assert.That(noMain.IsValid, Is.False);
            Assert.That(empty.IsValid, Is.False);
            Assert.That(negative.IsValid, Is.False);
        });
    }
}
