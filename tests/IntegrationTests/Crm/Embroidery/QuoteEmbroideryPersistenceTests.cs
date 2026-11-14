using Ardalis.Result;
using Crm.Domain.Quotes;
using Crm.Application.Features.Embroidery.Commands.SaveEmbroideryCalculation;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Quotes.Commands.AddEmbroideryToQuote;
using Crm.Application.Features.Quotes.Commands.CopyQuote;
using Crm.Application.Features.Quotes.Commands.CreateQuote;
using Crm.Application.Features.Quotes.Commands.SaveQuote;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Building;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Abstractions.Embroidery;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;
using Crm.Application.Abstractions.Quotes.Products;
using IntegrationTests.Crm.Quotes;

namespace IntegrationTests.Crm.Embroidery;

/// <summary>«Додати в КП» from «Ціна вишивки» over the real crm schema (EMB-001 R-12).</summary>
[NonParallelizable]
public sealed class QuoteEmbroideryPersistenceTests : CrmDatabaseFixture
{
    private static readonly QuoteActor Olena = new(17, false);
    private static readonly QuoteActor Petro = new(18, false);
    private readonly QuoteTestStaff _staff = new();
    private readonly QuoteTestClock _clock = new(new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero));
    private readonly QuoteItemsBuilder _builder = new(new NoCatalog());

    private static SaveQuoteInput OneOff(int version = 0) => new(version, "ТОВ «Агро»", null, null, null, null, null,
    [
        new QuoteItemInput(null, "OneOffPpe", null, "Рукавиці", null, null, null,
            [new QuoteOptionInput(null, null, false, new(true, 50m), new(false, null), new(false, null), null)]),
    ]);

    private async Task<long> SaveCalculation()
    {
        await using var db = Context();
        var saved = await new SaveEmbroideryCalculationHandler(db, new FixedStitchRate(), _staff, _clock)
            .Handle(new SaveEmbroideryCalculationCommand(new EmbroideryActor(17, false),
                new SaveEmbroideryCalculationInput("Логотип на поло", null,
                    new CalculateEmbroideryInput("Garment", 50, 10_000, "18", 1, 1, 1, "New", 60, null, "UsedBlanks",
                        null), "Категорія 2")), default);
        Assert.That(saved.IsSuccess, Is.True, string.Join(",", saved.ValidationErrors.Select(x => x.ErrorMessage)));
        return saved.Value.Summary.Id;
    }

    private async Task<QuoteResponse> CreateQuote()
    {
        await using var db = Context();
        return (await new CreateQuoteHandler(_staff, _builder, new QuoteNumbering(db), _clock)
            .Handle(new CreateQuoteCommand(Olena, OneOff()), default)).Value;
    }

    private async Task<Result<QuoteResponse>> Add(QuoteActor actor, long quoteId, long calculationId)
    {
        await using var db = Context();
        return await new AddEmbroideryToQuoteHandler(db, db, _staff, _clock)
            .Handle(new AddEmbroideryToQuoteCommand(actor, quoteId, calculationId), default);
    }

    [Test]
    public async Task AddedEmbroideryShowsTheSavedCategoryInEveryTierAndSurvivesSaveAndCopy()
    {
        var calculationId = await SaveCalculation();
        var quote = await CreateQuote();

        var added = await Add(Olena, quote.Id, calculationId);
        Assert.That(added.IsSuccess, Is.True, string.Join(",", added.Errors));
        var item = added.Value.Items[^1];
        Assert.That(item.Kind, Is.EqualTo("Embroidery"));
        Assert.That(item.EmbroideryCalculationId, Is.EqualTo(calculationId));
        Assert.That(item.Name, Is.EqualTo("Логотип на поло"));
        Assert.That(item.Subtitle, Is.EqualTo("Вишивка · ВШ-0001"));
        Assert.That(item.Description, Is.Null, "no quantity, stitches or hoop for the client");
        var option = item.Options.Single();
        Assert.That(option.Title, Is.EqualTo("Категорія 2"));
        Assert.That(new[] { option.Retail.Amount, option.Wholesale.Amount, option.Large.Amount }.Distinct().Single(),
            Is.GreaterThan(0m));
        Assert.That(option.Retail.Shown && option.Wholesale.Shown && option.Large.Shown, Is.True);
        Assert.That(added.Value.Version, Is.EqualTo(quote.Version + 1));

        var unit = option.Retail.Amount;
        var keep = new QuoteItemInput(item.Id, "Embroidery", null, null, "Інший опис", null, null,
            [new QuoteOptionInput(option.Id, null, false, new(true, null), new(false, null), new(true, null), null)]);
        var input = OneOff(added.Value.Version) with { Items = [OneOff().Items[0], keep] };
        await using (var db = Context())
        {
            var saved = await new SaveQuoteHandler(db, _staff, _builder, _clock)
                .Handle(new SaveQuoteCommand(Olena, quote.Id, input), default);
            Assert.That(saved.IsSuccess, Is.True, string.Join(",", saved.ValidationErrors.Select(x => x.ErrorMessage)));
            var kept = saved.Value.Items[^1];
            Assert.That(kept.Description, Is.EqualTo("Інший опис"));
            Assert.That(kept.Options[0].Wholesale.Shown, Is.False);
            Assert.That(kept.Options[0].Large.Amount, Is.EqualTo(unit));
            Assert.That(kept.EmbroideryCalculationId, Is.EqualTo(calculationId));

            var copy = await new CopyQuoteHandler(db, _staff, new QuoteNumbering(db), _clock)
                .Handle(new CopyQuoteCommand(Olena, quote.Id), default);
            Assert.That(copy.Value.Items[^1].EmbroideryCalculationId, Is.EqualTo(calculationId));
        }
    }

    [Test]
    public async Task NewEmbroideryCannotBeTypedInTheEditorAndOthersCannotAdd()
    {
        var calculationId = await SaveCalculation();
        var quote = await CreateQuote();

        Assert.That((await Add(Petro, quote.Id, calculationId)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await Add(Olena, quote.Id, 999)).Status, Is.EqualTo(ResultStatus.Invalid));

        var typed = new QuoteItemInput(null, "Embroidery", null, null, null, null, null,
            [new QuoteOptionInput(null, null, false, new(true, null), new(true, null), new(true, null), null)]);
        await using var db = Context();
        var saved = await new SaveQuoteHandler(db, _staff, _builder, _clock).Handle(
            new SaveQuoteCommand(Olena, quote.Id, OneOff(quote.Version) with { Items = [typed] }), default);
        Assert.That(saved.Status, Is.EqualTo(ResultStatus.Invalid));
    }

    private sealed class FixedStitchRate : IEmbroideryStitchRateReader
    {
        public Task<EmbroideryStitchRate> ReadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new EmbroideryStitchRate(1m, new DateOnly(2026, 8, 30)));
    }

    private sealed class NoCatalog : IQuoteCatalogReader
    {
        public Task<QuoteCatalogPage> SearchAsync(QuoteItemKind kind, string? search, string? season, int page,
            int pageSize, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyDictionary<int, QuoteCatalogProduct>> GetAsync(IReadOnlyCollection<int> productIds,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyDictionary<int, QuoteCatalogProduct>>(new Dictionary<int, QuoteCatalogProduct>());
    }
}
