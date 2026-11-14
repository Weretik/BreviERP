using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Building;
using Crm.Application.Features.Quotes.Shared.Introductions;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Features.Quotes.Shared.Settings.Reading;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.ValueObjects;

namespace Crm.Application.Features.Quotes.Shared.Introductions.Building;

/// <summary>
/// Turns the pages of a «КП для знайомства» into ordinary sewing items with one fabric and only the від 41 шт. price
/// shown (R-16), through <see cref="QuoteItemsBuilder"/>: a saved model keeps its price, a new one takes the Catalog
/// price. Then checks what needs the Catalog: the main model of each page, the seasons and repeats.
/// </summary>
public sealed class QuoteIntroductionBuilder(QuoteItemsBuilder items, IQuoteStore store)
{
    public async Task<Result<IReadOnlyList<QuoteItem>>> BuildAsync(
        QuoteIntroduction introduction, IReadOnlyList<QuoteIntroPageInput> pages, IReadOnlyList<QuoteItem> saved,
        QuotePerson actor, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var inputs = pages.SelectMany(page => page.Models).Select(model => ToItemInput(model, saved)).ToArray();
        var built = await items.BuildAsync(inputs, saved, actor, now, cancellationToken);
        if (!built.IsSuccess) return built;

        var texts = (await store.SettingsContentAsync(cancellationToken)).Introduction;
        foreach (var (season, pageItems) in QuoteIntroPages.Split(introduction, built.Value))
        {
            var problem = PageProblem(season, pageItems, texts.MainCode(season));
            if (problem is not null) return Result.Conflict(problem);
        }

        return built;
    }

    private static QuoteItemInput ToItemInput(QuoteIntroModelInput model, IReadOnlyList<QuoteItem> saved)
    {
        var item = saved.FirstOrDefault(x => x.Id == model.ItemId && x.ProductId == model.ProductId);
        var option = item?.Options.FirstOrDefault(x => x.ArticleId == model.ArticleId);
        var hidden = new QuotePriceInput(false, null);
        return new QuoteItemInput(item?.Id, nameof(QuoteItemKind.Sewing), model.ProductId, null, item?.Description,
            null, null,
            [new QuoteOptionInput(option?.Id, model.ArticleId, false, hidden, hidden, new QuotePriceInput(true, null), null)]);
    }

    private static string? PageProblem(QuoteIntroSeason season, IReadOnlyList<QuoteItem> pageItems, string mainCode)
    {
        var title = QuoteIntroPages.Title(season);
        if (!IsCode(pageItems[0], mainCode))
            return $"Сторінка «{title}»: першою має бути головна модель {mainCode}.";
        foreach (var item in pageItems.Skip(1))
        {
            if (IsCode(item, mainCode)) return $"Сторінка «{title}»: головна модель вже є на сторінці.";
            if (!QuoteIntroPages.IsSeason(item.Season, season))
                return $"Сторінка «{title}»: «{item.Name}» — не модель сезону «{QuoteIntroPages.CatalogSeason(season)}».";
        }

        return null;
    }

    private static bool IsCode(QuoteItem item, string code) =>
        string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase);
}
