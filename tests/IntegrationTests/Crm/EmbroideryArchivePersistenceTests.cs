using Ardalis.Result;
using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.GetCalculation;
using Crm.Application.Embroidery.ImportArchive;
using Crm.Application.Embroidery.Mapping;
using Crm.Application.Embroidery.StitchRate;
using Crm.Domain.Embroidery.Settings;

namespace IntegrationTests.Crm;

/// <summary>«Імпорт архіву» of the old calculator over the real crm schema (EMB-001 stage 2).</summary>
[NonParallelizable]
public sealed class EmbroideryArchivePersistenceTests : CrmDatabaseFixture
{
    private static readonly EmbroideryActor Director = new(1, true);
    private static readonly EmbroideryActor Olena = new(17, false);
    private static readonly DateTimeOffset OldDate = new(2026, 10, 26, 14, 55, 16, TimeSpan.Zero);
    private readonly QuoteTestStaff _staff = new();

    /// <summary>A real record of the old archive («енерджи групп», 78 pcs) with the old rates it was saved with.</summary>
    private static EmbroideryArchiveRow Row(string legacyId = "7ff8c09a", string? email = "o@brevi.test")
    {
        var settings = ((EmbroiderySettings?)null).ToResponse(new EmbroideryStitchRate(1m, null));
        return new EmbroideryArchiveRow(legacyId, "енерджи групп", OldDate, email,
            new CalculateEmbroideryInput("Garment", 78, 7000, "18", 1, 1, 1, "New", 60, null, "UsedBlanks",
                new Dictionary<string, bool> { ["edge"] = false, ["preparation"] = true }),
            1m,
            new SaveEmbroiderySettingsInput(0, settings.Rates, settings.Markups, settings.Stabilizer, settings.Fabric,
                settings.FabricIncreasePercent, settings.Hoops, settings.Expenses),
            [new(65.84m, 5135.52m, 58.21m), new(78.19m, 6098.82m, 69.13m), new(90.53m, 7061.34m, 80.04m)]);
    }

    private async Task<Result<EmbroideryArchiveImportResult>> Import(EmbroideryActor actor, bool dryRun,
        params EmbroideryArchiveRow[] rows)
    {
        await using var db = Context();
        return await new ImportEmbroideryArchiveCommandHandler(db, _staff)
            .Handle(new ImportEmbroideryArchiveCommand(actor, rows, dryRun), default);
    }

    private async Task<int> Count()
    {
        await using var db = Context();
        return db.EmbroideryCalculations.Count();
    }

    [Test]
    public async Task OldRecordIsMovedOnceWithItsDateAuthorAndPrices()
    {
        var check = (await Import(Director, true, Row(), Row("b2", "gone@old.test"))).Value;
        Assert.That(check.DryRun, Is.True);
        Assert.That(check.Created, Is.EqualTo(2));
        Assert.That(check.Lines.Select(x => x.CreatedByName), Is.EqualTo(new[] { "Олена", "gone@old.test" }));
        Assert.That(await Count(), Is.Zero, "A check saves nothing.");

        var moved = (await Import(Director, false, Row(), Row("b2", "gone@old.test"))).Value;
        Assert.That(moved.DryRun, Is.False);
        Assert.That(moved.Lines.Select(x => x.Number), Is.EqualTo(new[] { "ВШ-0001", "ВШ-0002" }));

        await using (var db = Context())
        {
            var saved = (await new GetEmbroideryCalculationQueryHandler(db, new EmbroiderySavedPersistenceTests.FakeMedia())
                .Handle(new GetEmbroideryCalculationQuery(Director, 1), default)).Value;
            Assert.That(saved.Summary.CreatedAt, Is.EqualTo(OldDate));
            Assert.That(saved.Summary.CreatedByName, Is.EqualTo("Олена"));
            Assert.That(saved.Summary.Prices.Select(x => x.Unit), Is.EqualTo(new[] { 65.84m, 78.19m, 90.53m }));
            Assert.That(saved.Rates!.StitchRate, Is.EqualTo(1m));
        }

        var again = (await Import(Director, false, Row(), Row("b2"))).Value;
        Assert.That(again.Unchanged, Is.EqualTo(2));
        Assert.That(again.Created, Is.Zero);
        Assert.That(await Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task DifferentPricesStopTheWholeList()
    {
        var wrong = Row("b2") with { Prices = [new(65.85m, 5135.52m, 58.21m), new(78.19m, 6098.82m, 69.13m),
            new(90.53m, 7061.34m, 80.04m)] };
        var result = (await Import(Director, false, Row(), wrong, Row())).Value;
        Assert.That(result.DryRun, Is.True);
        Assert.That(result.Errors, Is.EqualTo(2));
        Assert.That(result.Lines.Select(x => x.Action), Is.EqualTo(new[] { "created", "error", "error" }));
        Assert.That(await Count(), Is.Zero);
    }

    [Test]
    public async Task ManagerCannotImport()
    {
        Assert.That((await Import(Olena, true, Row())).Status, Is.EqualTo(ResultStatus.Forbidden));
    }
}
