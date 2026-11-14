using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Shared;

/// <summary>
/// Builds a «Виріб з нуля» (R-20) or a Catalog model with a changed composition (R-21). While the composition and
/// fabrics stay as saved, the item keeps its prices (R-11); any change prices the whole item again by the Catalog
/// formula. A changed model keeps its code, site link and season from the Catalog.
/// </summary>
public sealed class QuoteCustomSewingItemBuilder(IQuoteSewingCalculator calculator)
{
    public const string Subtitle = "Швейна продукція · виріб з нуля";
    public const string ChangedModelSubtitle = "Швейна продукція · змінений склад моделі";

    public async Task<Result<QuoteItem>> BuildAsync(
        int position, QuoteItemInput input, QuoteItem? previous, IReadOnlyDictionary<int, QuoteCatalogProduct> products,
        DateTimeOffset now, CancellationToken cancellationToken)
    {
        var name = input.Name!.Trim();
        QuoteCatalogProduct? model = null;
        if (input.ProductId is { } productId && previous is null &&
            (!products.TryGetValue(productId, out model) || model.Kind != QuoteItemKind.Sewing))
            return Result.Conflict($"«{name}»: модель не знайдено в каталозі.");

        var texts = Texts(input, name, previous, model);
        var requested = (input.Lines ?? []).Select(x => x.ToLine()).ToArray();
        var kept = previous is not null ? Keep(position, input, previous, requested, texts) : null;
        if (kept is not null) return Result.Success(kept);

        var composition = new QuoteSewingComposition(input.Options.Select(x => x.Fabrics!.ToVariant()).ToArray(),
            requested);
        var calculation = await calculator.CalculateAsync(composition, cancellationToken);

        var lines = new List<QuoteItemLine>(requested.Length);
        for (var index = 0; index < requested.Length; index++)
        {
            var info = calculation.Lines[index];
            if (info.Name is null)
                return Result.Conflict($"«{name}»: {Missing(info.Kind)} не знайдено в довіднику.");
            var line = requested[index];
            lines.Add(new QuoteItemLine(index, line.Kind, line.ReferenceId, info.Name, info.Unit ?? "", line.Quantity,
                line.ManualMinutes, line.CostCode));
        }

        var options = new List<QuoteItemOption>(input.Options.Count);
        for (var index = 0; index < input.Options.Count; index++)
        {
            var priced = Priced(index, input.Options[index], calculation.Variants[index], name, now);
            if (!priced.IsSuccess) return priced.Map(_ => default(QuoteItem)!);
            options.Add(priced.Value);
        }

        return Result.Success(new QuoteItem(position, QuoteItemKind.CustomSewing, input.ProductId, texts, options, lines));
    }

    /// <summary>A variant priced by the formula: its fabrics summed with the shared lines (R-26).</summary>
    public static Result<QuoteItemOption> Priced(int position, QuoteOptionInput input, QuoteSewingVariantPrice variant,
        string itemName, DateTimeOffset now)
    {
        if (variant.Title is not { } title) return Result.Conflict($"«{itemName}»: тканину не знайдено в довіднику.");
        if (variant.Prices is not { } prices)
            return Result.Conflict($"«{itemName}», «{title}»: не вдалося порахувати ціну ({string.Join("; ", variant.Problems)}).");
        return Result.Success(new QuoteItemOption(position, null, variant.Fabrics[0].FabricId, Shorten(title),
                variant.Details, input.IsRecommended,
                new QuotePrice(prices.Retail, null, input.Retail.Shown),
                new QuotePrice(prices.Wholesale, null, input.Wholesale.Shown),
                new QuotePrice(prices.Large, null, input.Large.Shown), now)
            .WithFabrics(variant.Fabrics.Select((x, i) => new QuoteItemOptionFabric(i, x.FabricId, x.Name!, x.Meters))));
    }

    private static string Shorten(string title) =>
        title.Length <= QuoteLimits.NameMaxLength ? title : title[..(QuoteLimits.NameMaxLength - 1)] + "…";

    private static string Missing(QuoteItemLineKind kind) => kind switch
    {
        QuoteItemLineKind.Operation => "операцію",
        QuoteItemLineKind.Service => "послугу",
        QuoteItemLineKind.Fabric => "тканину",
        _ => "фурнітуру"
    };

    /// <summary>The saved item with new texts and shown flags, or null when its composition or fabrics changed.</summary>
    private static QuoteItem? Keep(
        int position, QuoteItemInput input, QuoteItem previous, IReadOnlyList<QuoteSewingLine> requested,
        QuoteItemTexts texts)
    {
        var savedLines = previous.Lines.OrderBy(x => x.Position).ToArray();
        if (savedLines.Length != requested.Count) return null;
        for (var index = 0; index < requested.Count; index++)
        {
            var line = requested[index];
            var saved = savedLines[index];
            if (saved.Kind != line.Kind || saved.ReferenceId != (line.Kind == QuoteItemLineKind.Service ? 0 : line.ReferenceId) ||
                saved.CostCode != line.CostCode || saved.Quantity != line.Quantity ||
                saved.ManualMinutes != line.ManualMinutes) return null;
        }

        var options = new List<QuoteItemOption>(input.Options.Count);
        for (var index = 0; index < input.Options.Count; index++)
        {
            var optionInput = input.Options[index];
            var saved = previous.Options.FirstOrDefault(x => x.Id == optionInput.Id);
            if (saved is null || !saved.HasFabrics(optionInput.Fabrics!.Select(x => (x.FabricId, x.Meters)).ToArray()))
                return null;
            options.Add(new QuoteItemOption(index, null, saved.FabricId, saved.Title, saved.Details,
                    optionInput.IsRecommended, saved.Retail with { Shown = optionInput.Retail.Shown },
                    saved.Wholesale with { Shown = optionInput.Wholesale.Shown },
                    saved.Large with { Shown = optionInput.Large.Shown }, saved.PricesTakenAt)
                .WithFabrics(saved.Fabrics.OrderBy(x => x.Position).Select(x => x.Copy())));
        }

        var lines = savedLines.Select((x, index) =>
            new QuoteItemLine(index, x.Kind, x.ReferenceId, x.Name, x.Unit, x.Quantity, x.ManualMinutes, x.CostCode));
        return new QuoteItem(position, QuoteItemKind.CustomSewing, previous.ProductId, texts, options, lines);
    }

    private static QuoteItemTexts Texts(QuoteItemInput input, string name, QuoteItem? previous, QuoteCatalogProduct? model) =>
        input.ProductId is null
            ? new(null, name, Subtitle, input.Description, null, input.PhotoUrl, null, null)
            : new(previous?.Code ?? model?.Code, name, ChangedModelSubtitle, input.Description,
                previous?.SiteUrl ?? model?.SiteUrl, input.PhotoUrl ?? previous?.PhotoUrl ?? model?.PhotoUrl,
                previous?.Season ?? model?.Season, null);
}
