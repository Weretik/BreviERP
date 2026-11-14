using Ardalis.Result;
using Crm.Application.Features.Embroidery.Commands.DeleteEmbroideryCalculation;
using Crm.Application.Features.Embroidery.Commands.SaveEmbroideryCalculation;
using Crm.Application.Features.Embroidery.Commands.SaveEmbroiderySettings;
using Crm.Application.Features.Embroidery.Commands.SetEmbroideryMockup;
using Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculation;
using Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculations;
using Crm.Application.Features.Embroidery.Queries.GetEmbroideryMockupUsages;
using Crm.Application.Features.Embroidery.Queries.GetEmbroiderySettings;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Abstractions.Embroidery;
using IntegrationTests.Crm.Quotes;

namespace IntegrationTests.Crm.Embroidery;

/// <summary>«Ціна вишивки › Збережені» over the real crm schema (EMB-001 stage 2).</summary>
[NonParallelizable]
public sealed class EmbroiderySavedPersistenceTests : CrmDatabaseFixture
{
    private static readonly EmbroideryActor Director = new(1, true);
    private static readonly EmbroideryActor Olena = new(17, false);
    private readonly QuoteTestStaff _staff = new();
    private readonly QuoteTestClock _clock = new(new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero));
    private readonly FixedStitchRate _stitchRate = new();
    private readonly FakeMedia _media = new();

    private static CalculateEmbroideryInput Form() =>
        new("Garment", 50, 10_000, "18", 1, 1, 1, "New", 60, null, "UsedBlanks", null);

    private async Task<Result<EmbroiderySavedCalculationResponse>> Save(EmbroideryActor actor,
        SaveEmbroideryCalculationInput input)
    {
        await using var db = Context();
        return await new SaveEmbroideryCalculationHandler(db, _stitchRate, _staff, _clock)
            .Handle(new SaveEmbroideryCalculationCommand(actor, input), default);
    }

    private async Task<Result<EmbroiderySavedCalculationResponse>> Get(EmbroideryActor actor, long id)
    {
        await using var db = Context();
        return await new GetEmbroideryCalculationHandler(db, _media)
            .Handle(new GetEmbroideryCalculationQuery(actor, id), default);
    }

    private async Task<EmbroiderySavedPageResponse> List(string? search)
    {
        await using var db = Context();
        return (await new GetEmbroideryCalculationsHandler(db, _media)
            .Handle(new GetEmbroideryCalculationsQuery(Olena, search), default)).Value;
    }

    private async Task RaiseHoopingRate(decimal rate)
    {
        await using var db = Context();
        var current = (await new GetEmbroiderySettingsHandler(db, _stitchRate)
            .Handle(new GetEmbroiderySettingsQuery(Director), default)).Value;
        var input = new SaveEmbroiderySettingsInput(current.Version, current.Rates with { HoopingRate = rate },
            current.Markups, current.Stabilizer, current.Fabric, current.FabricIncreasePercent, current.Hoops,
            current.Expenses);
        var saved = await new SaveEmbroiderySettingsHandler(db, _stitchRate, _staff, _clock)
            .Handle(new SaveEmbroiderySettingsCommand(Director, input), default);
        Assert.That(saved.IsSuccess, Is.True, string.Join(",", saved.Errors));
    }

    [Test]
    public async Task SavedCalculationKeepsItsRatesAfterTheSettingsChange()
    {
        await RaiseHoopingRate(12m);
        var saved = await Save(Olena, new SaveEmbroideryCalculationInput("  Логотип на поло  ", null, Form(), "Категорія 1"));
        Assert.That(saved.IsSuccess, Is.True, string.Join(",", saved.Errors));
        Assert.That(saved.Value.Summary.Number, Is.EqualTo("ВШ-0001"));
        Assert.That(saved.Value.Summary.Name, Is.EqualTo("Логотип на поло"));
        Assert.That(saved.Value.Summary.CreatedByName, Is.EqualTo("Олена"));
        Assert.That(saved.Value.Result.Cost, Is.Null, "A Manager never gets the cost.");
        Assert.That(saved.Value.Rates, Is.Null, "A Manager never gets the frozen rates.");
        var total = saved.Value.Summary.Price!.Total;
        Assert.That(saved.Value.Summary.Price.Name, Is.EqualTo("Категорія 1"));
        Assert.That(saved.Value.Result.Prices.Select(x => x.Name), Is.EqualTo(new[] { "Категорія 1" }));

        await RaiseHoopingRate(30m);

        var reloaded = (await Get(Director, saved.Value.Summary.Id)).Value;
        Assert.That(reloaded.Rates!.Rates.HoopingRate, Is.EqualTo(12m));
        Assert.That(reloaded.Result.Cost!.Rows.Single(x => x.Kind == "Hooping").Amount, Is.EqualTo(600m));
        Assert.That(reloaded.Summary.Price!.Total, Is.EqualTo(total));
        Assert.That(reloaded.Input, Is.EqualTo(Form()).Using<CalculateEmbroideryInput>((a, b) =>
            a.Mode == b.Mode && a.Quantity == b.Quantity && a.HoopId == b.HoopId && a.Program == b.Program));
    }

    [Test]
    public async Task ListIsNewestFirstAndSearchesByNameAndNumber()
    {
        Assert.That((await Save(Olena, new SaveEmbroideryCalculationInput("Поло", null, Form(), "Категорія 1"))).IsSuccess, Is.True);
        Assert.That((await Save(Olena, new SaveEmbroideryCalculationInput(null, null,
            Form() with { Quantity = 10 }, "Категорія 1"))).IsSuccess, Is.True);

        var all = await List(null);
        Assert.That(all.Total, Is.EqualTo(2));
        Assert.That(all.Items.Select(x => x.Number), Is.EqualTo(new[] { "ВШ-0002", "ВШ-0001" }));
        Assert.That(all.Items[0].HoopLabel, Is.Not.Empty);

        Assert.That((await List("поло")).Items.Select(x => x.Id), Is.EqualTo(new long[] { 1 }));
        Assert.That((await List("вш-0002")).Items.Select(x => x.Id), Is.EqualTo(new long[] { 2 }));
        Assert.That((await List("нічого")).Total, Is.Zero);
    }

    [Test]
    public async Task UnknownCustomerAndMissingCalculationAreRejected()
    {
        var unknown = await Save(Olena, new SaveEmbroideryCalculationInput(null, Guid.NewGuid(), Form(), "Категорія 1"));
        Assert.That(unknown.Status, Is.EqualTo(ResultStatus.Invalid));
        var noCategory = await Save(Olena, new SaveEmbroideryCalculationInput(null, null, Form(), "Опт"));
        Assert.That(noCategory.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That((await Get(Olena, 99)).Status, Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task OnlyDirectorOrAdminDeletes()
    {
        var saved = (await Save(Olena, new SaveEmbroideryCalculationInput("Поло", null, Form(), "Категорія 1"))).Value;

        await using (var db = Context())
        {
            var handler = new DeleteEmbroideryCalculationHandler(db);
            Assert.That((await handler.Handle(new DeleteEmbroideryCalculationCommand(Olena, saved.Summary.Id), default))
                .Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That((await handler.Handle(new DeleteEmbroideryCalculationCommand(Director, saved.Summary.Id),
                default)).IsSuccess, Is.True);
        }

        Assert.That((await Get(Director, saved.Summary.Id)).Status, Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task MockupIsPickedFromTheMediaLibraryAndBlocksItsDeletion()
    {
        var saved = (await Save(Olena, new SaveEmbroideryCalculationInput("Поло", null, Form(), "Категорія 1"))).Value;
        _media.Files[5] = new EmbroideryMockupResponse(5, "logo.png", "https://cdn.test/logo.png", null);

        await using (var db = Context())
        {
            var handler = new SetEmbroideryMockupHandler(db, _media);
            var missing = await handler.Handle(new SetEmbroideryMockupCommand(Olena, saved.Summary.Id,
                new SetEmbroideryMockupInput(99)), default);
            var set = await handler.Handle(new SetEmbroideryMockupCommand(Olena, saved.Summary.Id,
                new SetEmbroideryMockupInput(5)), default);
            Assert.That(missing.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(set.Value.Summary.Mockup!.Url, Is.EqualTo("https://cdn.test/logo.png"));
        }

        Assert.That((await Get(Olena, saved.Summary.Id)).Value.Summary.Mockup!.MediaFileId, Is.EqualTo(5));
        Assert.That((await List(null)).Items.Single().Mockup!.FileName, Is.EqualTo("logo.png"));
        await using (var db = Context())
        {
            var usages = (await new GetEmbroideryMockupUsagesHandler(db)
                .Handle(new GetEmbroideryMockupUsagesQuery([5, 6]), default)).Value;
            Assert.That(usages.Single().Title, Is.EqualTo(saved.Summary.Number + " Поло"));

            var cleared = await new SetEmbroideryMockupHandler(db, _media).Handle(
                new SetEmbroideryMockupCommand(Olena, saved.Summary.Id, new SetEmbroideryMockupInput(null)), default);
            Assert.That(cleared.Value.Summary.Mockup, Is.Null);
        }
    }

    internal sealed class FakeMedia : IEmbroideryMediaReader
    {
        public Dictionary<int, EmbroideryMockupResponse> Files { get; } = [];

        public Task<IReadOnlyDictionary<int, EmbroideryMockupResponse>> ReadAsync(
            IReadOnlyCollection<int> mediaFileIds, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyDictionary<int, EmbroideryMockupResponse>>(Files
                .Where(x => mediaFileIds.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value));
    }

    private sealed class FixedStitchRate : IEmbroideryStitchRateReader
    {
        public Task<EmbroideryStitchRate> ReadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new EmbroideryStitchRate(1m, new DateOnly(2026, 8, 30)));
    }
}
