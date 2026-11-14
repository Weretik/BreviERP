using Ardalis.Result;
using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Commands.CreateQuoteIntroTemplate;
using Crm.Application.Features.Quotes.Commands.CreateQuoteIntroduction;
using Crm.Application.Features.Quotes.Commands.DeleteQuoteIntroTemplate;
using Crm.Application.Features.Quotes.Commands.SaveQuote;
using Crm.Application.Features.Quotes.Commands.SaveQuoteIntroduction;
using Crm.Application.Features.Quotes.Queries.GetQuoteIntroTemplates;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Building;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Features.Quotes.Shared.Introductions;
using Crm.Application.Features.Quotes.Shared.Introductions.Building;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;
using Crm.Application.Abstractions.Quotes.Products;

namespace IntegrationTests.Crm.Quotes;

/// <summary>«КП для знайомства» and «Готові шаблони» over the real crm schema (R-16–R-19).</summary>
[NonParallelizable]
public sealed class QuoteIntroductionPersistenceTests : CrmDatabaseFixture
{
    private static readonly QuoteActor Olena = new(17, false);
    private static readonly QuoteActor Petro = new(18, false);
    private static readonly QuoteActor Director = new(1, true);

    private readonly IntroCatalog _catalog = new();
    private readonly QuoteTestStaff _staff = new();
    private readonly QuoteTestClock _clock = new(new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero));

    private static QuoteIntroPageInput Page(string season, params int[] products) =>
        new(season, products.Select(x => new QuoteIntroModelInput(null, x, x * 100L)).ToArray());

    private static SaveQuoteIntroductionInput General(int version = 0) =>
        new(version, "ТОВ «Агро» — знайомство", null, "Office@Agro.UA", "Іван", "ТОВ «Агро»", null, "General",
            "Після розмови", [Page("Summer", 1, 2, 3, 4), Page("Winter", 5, 6, 7, 8)]);

    private async Task<Result<QuoteResponse>> Create(SaveQuoteIntroductionInput input)
    {
        await using var db = Context();
        var builder = new QuoteIntroductionBuilder(new QuoteItemsBuilder(_catalog), db);
        return await new CreateQuoteIntroductionHandler(_staff, builder, new QuoteNumbering(db), _clock)
            .Handle(new CreateQuoteIntroductionCommand(Olena, input), default);
    }

    [Test]
    public async Task IntroductionKeepsPagesInOrderWithOnlyTheLargePriceShown()
    {
        var created = await Create(General());

        Assert.That(created.IsSuccess, Is.True, string.Join(",", created.Errors));
        var quote = created.Value;
        Assert.That(quote.Kind, Is.EqualTo("Introduction"));
        Assert.That(quote.Introduction!.Layout, Is.EqualTo("General"));
        Assert.That(quote.Introduction.RecipientEmail, Is.EqualTo("office@agro.ua"));
        Assert.That(quote.Items.Select(x => x.ProductId), Is.EqualTo(new int?[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
        var option = quote.Items[0].Options.Single();
        Assert.That((option.Retail.Shown, option.Wholesale.Shown, option.Large.Shown), Is.EqualTo((false, false, true)));
        Assert.That(option.Large.Amount, Is.EqualTo(800m));
    }

    [Test]
    public async Task PageMustStartWithTheMainModelOfItsSeason()
    {
        var wrongMain = await Create(General() with { Pages = [Page("Summer", 2, 1, 3, 4), Page("Winter", 5, 6, 7, 8)] });
        var wrongSeason = await Create(General() with { Layout = "Summer", Pages = [Page("Summer", 1, 2, 3, 6)] });

        Assert.That(wrongMain.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(wrongMain.Errors.Single(), Does.Contain("MOD-166"));
        Assert.That(wrongSeason.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(wrongSeason.Errors.Single(), Does.Contain("Зима").Or.Contain("Літо"));
    }

    [Test]
    public async Task SavedModelKeepsItsPriceAndRequestEditorRefusesIntroduction()
    {
        var created = (await Create(General())).Value;
        _catalog.Large = 700;
        var pages = created.Items.Select(x => new QuoteIntroModelInput(x.Id, x.ProductId!.Value, x.Options[0].ArticleId!.Value))
            .Chunk(4).Select((models, index) => new QuoteIntroPageInput(index == 0 ? "Summer" : "Winter", models)).ToArray();

        await using var db = Context();
        var builder = new QuoteIntroductionBuilder(new QuoteItemsBuilder(_catalog), db);
        var saved = await new SaveQuoteIntroductionHandler(db, _staff, builder, _clock)
            .Handle(new SaveQuoteIntroductionCommand(Olena, created.Id, General(created.Version) with { Pages = pages }), default);
        var asRequest = await new SaveQuoteHandler(db, _staff, new QuoteItemsBuilder(_catalog), _clock)
            .Handle(new SaveQuoteCommand(Olena, created.Id,
                new SaveQuoteInput(saved.Value.Version, "x", null, null, null, null, null, [])), default);

        Assert.That(saved.IsSuccess, Is.True, string.Join(",", saved.Errors));
        Assert.That(saved.Value.Items[0].Options[0].Large.Amount, Is.EqualTo(800m));
        Assert.That(asRequest.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(asRequest.Errors.Single(), Does.Contain("іншого виду"));
    }

    [Test]
    public async Task TemplatesAreSharedUniqueByNameAndDeletedByAuthorOrDirector()
    {
        var input = new CreateQuoteIntroTemplateInput("Агро база", "Summer", null, [Page("Summer", 1, 2, 3, 4)]);
        await using var db = Context();
        var create = new CreateQuoteIntroTemplateHandler(db, _staff, _catalog, _clock);
        var first = await create.Handle(new CreateQuoteIntroTemplateCommand(Olena, input), default);
        var duplicate = await create.Handle(new CreateQuoteIntroTemplateCommand(Olena, input with { Name = " агро БАЗА " }), default);

        await using var read = Context();
        var forPetro = await new GetQuoteIntroTemplatesHandler(read).Handle(new GetQuoteIntroTemplatesQuery(Petro), default);
        var byPetro = await new DeleteQuoteIntroTemplateHandler(read)
            .Handle(new DeleteQuoteIntroTemplateCommand(Petro, first.Value.Id), default);
        var byDirector = await new DeleteQuoteIntroTemplateHandler(read)
            .Handle(new DeleteQuoteIntroTemplateCommand(Director, first.Value.Id), default);

        Assert.That(first.IsSuccess, Is.True, string.Join(",", first.Errors));
        Assert.That(first.Value.Pages[0].Models[0].ProductName, Is.EqualTo("Модель 1"));
        Assert.That(first.Value.Pages[0].Models[0].FabricTitle, Is.EqualTo("Тканина 1"));
        Assert.That(duplicate.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(forPetro.Value.Single().CanDelete, Is.False);
        Assert.That(byPetro.Status, Is.EqualTo(ResultStatus.Forbidden));
        Assert.That(byDirector.IsSuccess, Is.True);
    }

    /// <summary>Models 1–4 are summer (1 = MOD-166), 5–8 winter (5 = MOD-169); fabric article = product × 100.</summary>
    private sealed class IntroCatalog : IQuoteCatalogReader
    {
        public decimal Large { get; set; } = 800;

        private QuoteCatalogProduct Model(int id) => new(id, QuoteItemKind.Sewing,
            id switch { 1 => "MOD-166", 5 => "MOD-169", _ => $"MOD-{id}" }, $"Модель {id}", id <= 4 ? "Літо" : "Зима",
            true, null, null, null, null, null, [],
            [new QuoteCatalogVariant(id * 100L, $"SKU-{id}", true, id, $"Тканина {id}", null, null,
                new QuoteTierPrices(Large + 200, Large + 100, Large), [])]);

        public Task<QuoteCatalogPage> SearchAsync(QuoteItemKind kind, string? search, string? season, int page,
            int pageSize, CancellationToken cancellationToken) =>
            Task.FromResult(new QuoteCatalogPage(Enumerable.Range(1, 8).Select(Model).ToArray(), page, pageSize, 8));

        public Task<IReadOnlyDictionary<int, QuoteCatalogProduct>> GetAsync(IReadOnlyCollection<int> productIds,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyDictionary<int, QuoteCatalogProduct>>(
                productIds.Where(x => x is >= 1 and <= 8).ToDictionary(x => x, Model));
    }
}
