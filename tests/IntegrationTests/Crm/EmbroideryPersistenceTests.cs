using Ardalis.Result;
using Crm.Application.Features.Embroidery.Commands.SaveEmbroiderySettings;
using Crm.Application.Features.Embroidery.Queries.CalculateEmbroidery;
using Crm.Application.Features.Embroidery.Queries.GetEmbroiderySettings;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Abstractions.Embroidery;

namespace IntegrationTests.Crm;

/// <summary>«Ціна вишивки › Налаштування» over the real crm schema (EMB-001 stage 1).</summary>
[NonParallelizable]
public sealed class EmbroideryPersistenceTests : CrmDatabaseFixture
{
    private static readonly EmbroideryActor Director = new(1, true);
    private static readonly EmbroideryActor Olena = new(17, false);
    private readonly QuoteTestStaff _staff = new();
    private readonly QuoteTestClock _clock = new(new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero));
    private readonly FixedStitchRate _stitchRate = new();

    private async Task<EmbroiderySettingsResponse> Load()
    {
        await using var db = Context();
        return (await new GetEmbroiderySettingsHandler(db, _stitchRate)
            .Handle(new GetEmbroiderySettingsQuery(Director), default)).Value;
    }

    private async Task<Result<EmbroiderySettingsResponse>> Save(EmbroideryActor actor, SaveEmbroiderySettingsInput input)
    {
        await using var db = Context();
        return await new SaveEmbroiderySettingsHandler(db, _stitchRate, _staff, _clock)
            .Handle(new SaveEmbroiderySettingsCommand(actor, input), default);
    }

    private static SaveEmbroiderySettingsInput ToInput(EmbroiderySettingsResponse x) =>
        new(x.Version, x.Rates, x.Markups, x.Stabilizer, x.Fabric, x.FabricIncreasePercent, x.Hoops, x.Expenses);

    [Test]
    public async Task FirstSaveStoresTheSettingsAndLaterSavesNeedTheLoadedVersion()
    {
        var defaults = await Load();
        Assert.That(defaults.Version, Is.Zero);
        Assert.That(defaults.StitchRate.Value, Is.EqualTo(1m));

        var input = ToInput(defaults) with { Rates = defaults.Rates with { HoopingRate = 12m } };
        input = input with { Expenses = [input.Expenses[0] with { Rate = null }, .. input.Expenses.Skip(1)] };
        var saved = await Save(Director, input);
        Assert.That(saved.IsSuccess, Is.True, string.Join(",", saved.Errors));

        var reloaded = await Load();
        Assert.That(reloaded.Version, Is.EqualTo(1));
        Assert.That(reloaded.UpdatedByName, Is.EqualTo("Директор"));
        Assert.That(reloaded.Rates.HoopingRate, Is.EqualTo(12m));
        Assert.That(reloaded.Expenses[0].Rate, Is.Null);
        Assert.That(reloaded.Expenses[0].Basis, Is.EqualTo("Hoop"));

        Assert.That((await Save(Director, input)).Status, Is.EqualTo(ResultStatus.Conflict));
    }

    [Test]
    public async Task ManagerCannotReadOrChangeTheSettings()
    {
        await using var db = Context();
        var read = await new GetEmbroiderySettingsHandler(db, _stitchRate)
            .Handle(new GetEmbroiderySettingsQuery(Olena), default);
        Assert.That(read.Status, Is.EqualTo(ResultStatus.Forbidden));
        Assert.That((await Save(Olena, ToInput(await Load()))).Status, Is.EqualTo(ResultStatus.Forbidden));
    }

    [Test]
    public async Task CalculationUsesTheSavedSettings()
    {
        var input = ToInput(await Load());
        Assert.That((await Save(Director, input with { Rates = input.Rates with { HoopingRate = 12m } })).IsSuccess, Is.True);

        await using var db = Context();
        var result = await new CalculateEmbroideryHandler(db, _stitchRate).Handle(new CalculateEmbroideryQuery(Director,
            new CalculateEmbroideryInput("Garment", 50, 10_000, "18", 1, 1, 1, "New", 60, null, "UsedBlanks", null)), default);

        Assert.That(result.Value.Cost!.Rows.Single(x => x.Kind == "Hooping").Amount, Is.EqualTo(600m));
    }

    private sealed class FixedStitchRate : IEmbroideryStitchRateReader
    {
        public Task<EmbroideryStitchRate> ReadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new EmbroideryStitchRate(1m, new DateOnly(2026, 8, 30)));
    }
}
