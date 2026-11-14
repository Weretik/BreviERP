using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Features.Quotes.Shared.Embroidery;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Shared.Building;

/// <summary>
/// Turns the editor content into items (R-05–R-10). A new Catalog row takes its texts and prices from the Catalog;
/// a saved row keeps them, so a save never reprices (R-11). Manual prices are accepted only for PPE. A «Виріб з нуля»
/// is priced by <see cref="QuoteCustomSewingItemBuilder"/>. A model row in another fabric from the Reference (R-27) is
/// priced by the same formula from the model's shared lines.
/// </summary>
public sealed class QuoteItemsBuilder(
    IQuoteCatalogReader catalog, IQuoteSewingCalculator? sewing = null, IQuoteModelCompositionReader? models = null)
{
    public const string SewingSubtitle = "Швейна продукція · готові варіанти тканини";
    public const string PpeSubtitle = "ЗІЗ · готовий товар";
    public const string OneOffSubtitle = "ЗІЗ · разова позиція";

    public async Task<Result<IReadOnlyList<QuoteItem>>> BuildAsync(
        IReadOnlyList<QuoteItemInput> inputs, IReadOnlyList<QuoteItem> saved, QuotePerson actor, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var productIds = inputs.Where(x => x.ProductId is > 0).Select(x => x.ProductId!.Value).Distinct().ToArray();
        var products = productIds.Length == 0
            ? new Dictionary<int, QuoteCatalogProduct>()
            : await catalog.GetAsync(productIds, cancellationToken);

        var items = new List<QuoteItem>(inputs.Count);
        for (var index = 0; index < inputs.Count; index++)
        {
            var input = inputs[index];
            var kind = Enum.Parse<QuoteItemKind>(input.Kind);
            var previous = saved.FirstOrDefault(x => x.Id == input.Id && x.Kind == kind && x.ProductId == input.ProductId);
            var built = kind switch
            {
                QuoteItemKind.OneOffPpe => BuildOneOff(index, input, previous, actor, now),
                QuoteItemKind.Embroidery => KeepEmbroidery(index, input, previous),
                QuoteItemKind.CustomSewing when sewing is null => Result.Error("Розрахунок виробу з нуля недоступний."),
                QuoteItemKind.CustomSewing => await new QuoteCustomSewingItemBuilder(sewing)
                    .BuildAsync(index, input, previous, products, now, cancellationToken),
                QuoteItemKind.Sewing => await BuildModelAsync(index, input, previous, products, actor, now,
                    cancellationToken),
                _ => BuildCatalogItem(index, kind, input, previous, products, actor, now, NoOtherFabrics)
            };
            if (!built.IsSuccess) return built.Map(_ => (IReadOnlyList<QuoteItem>)[]);
            if (!built.Value.ShowsPositivePrice)
                return InvalidItems($"«{built.Value.Name}»: покажіть клієнту хоча б одну ціну більше нуля.");
            items.Add(built.Value);
        }

        return Result.Success<IReadOnlyList<QuoteItem>>(items);
    }

    private static readonly IReadOnlyDictionary<int, QuoteSewingVariantPrice> NoOtherFabrics =
        new Dictionary<int, QuoteSewingVariantPrice>();

    /// <summary>
    /// Prices the rows in another fabric that are new or whose fabric changed, then builds the model like any Catalog
    /// item.
    /// </summary>
    private async Task<Result<QuoteItem>> BuildModelAsync(
        int position, QuoteItemInput input, QuoteItem? previous,
        IReadOnlyDictionary<int, QuoteCatalogProduct> products, QuotePerson actor, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var toPrice = input.Options.Select((option, index) => (option, index))
            .Where(x => x.option.ArticleId is null && x.option.Fabrics is { Count: > 0 } &&
                        SavedOtherFabric(previous, x.option) is null)
            .ToArray();
        if (toPrice.Length == 0)
            return BuildCatalogItem(position, QuoteItemKind.Sewing, input, previous, products, actor, now, NoOtherFabrics);

        var name = previous?.Name ?? (products.TryGetValue(input.ProductId!.Value, out var product) ? product.Name : "Модель");
        if (sewing is null || models is null) return Result.Error("Розрахунок іншої тканини недоступний.");
        var model = await models.ReadAsync(input.ProductId!.Value, cancellationToken);
        if (model is null) return Result.Conflict($"«{name}»: модель не знайдено в каталозі.");

        var lines = model.Lines
            .Select(x => new QuoteSewingLine(x.Kind, x.ReferenceId, x.Quantity, x.ManualMinutes, x.CostCode)).ToArray();
        var calculation = await sewing.CalculateAsync(
            new QuoteSewingComposition(toPrice.Select(x => x.option.Fabrics!.ToVariant()).ToArray(), lines),
            cancellationToken);
        var prices = toPrice.Select((x, i) => (x.index, price: calculation.Variants[i]))
            .ToDictionary(x => x.index, x => x.price);
        return BuildCatalogItem(position, QuoteItemKind.Sewing, input, previous, products, actor, now, prices);
    }

    /// <summary>A saved row in another fabric that the editor sends back unchanged, or an imported row without one.</summary>
    private static QuoteItemOption? SavedOtherFabric(QuoteItem? previous, QuoteOptionInput input)
    {
        var saved = previous?.Options.FirstOrDefault(x => x.Id == input.Id && x.ArticleId is null);
        if (saved is null) return null;
        if (input.Fabrics is not { Count: > 0 } fabrics) return saved;
        return saved.HasFabrics(fabrics.Select(x => (x.FabricId, x.Meters)).ToArray()) ? saved : null;
    }

    private static Result<QuoteItem> BuildCatalogItem(
        int position, QuoteItemKind kind, QuoteItemInput input, QuoteItem? previous,
        IReadOnlyDictionary<int, QuoteCatalogProduct> products, QuotePerson actor, DateTimeOffset now,
        IReadOnlyDictionary<int, QuoteSewingVariantPrice> otherFabrics)
    {
        products.TryGetValue(input.ProductId!.Value, out var product);
        if (previous is null)
        {
            if (product is null || product.Kind != kind) return Result.Conflict("Товар не знайдено в каталозі.");
            if (!product.IsActive) return Result.Conflict($"Товар «{product.Name}» вимкнений у каталозі.");
        }

        var name = previous?.Name ?? product!.Name;
        var texts = new QuoteItemTexts(
            previous?.Code ?? product!.Code,
            name,
            kind == QuoteItemKind.Sewing ? SewingSubtitle : PpeSubtitle,
            input.Description,
            previous?.SiteUrl ?? product!.SiteUrl,
            input.PhotoUrl ?? previous?.PhotoUrl ?? product?.PhotoUrl,
            previous?.Season ?? product!.Season,
            previous?.SupplierUrl ?? product?.SupplierUrl);

        var options = new List<QuoteItemOption>(input.Options.Count);
        for (var index = 0; index < input.Options.Count; index++)
        {
            var optionInput = input.Options[index];
            var option = kind != QuoteItemKind.Sewing
                ? BuildPpeRow(index, optionInput, previous, product, name, actor, now)
                : otherFabrics.TryGetValue(index, out var priced)
                    ? QuoteCustomSewingItemBuilder.Priced(index, optionInput, priced, name, now)
                    : BuildVariant(index, optionInput, previous, product, name, now);
            if (!option.IsSuccess) return option.Map(_ => default(QuoteItem)!);
            options.Add(option.Value);
        }

        return Result.Success(new QuoteItem(position, kind, input.ProductId, texts, options));
    }

    private static Result<QuoteItemOption> BuildVariant(
        int position, QuoteOptionInput input, QuoteItem? previous, QuoteCatalogProduct? product, string itemName,
        DateTimeOffset now)
    {
        var saved = input.ArticleId is null
            ? SavedOtherFabric(previous, input)
            : previous?.Options.FirstOrDefault(x => x.Id == input.Id && x.ArticleId == input.ArticleId);
        if (saved is not null)
            return Result.Success(new QuoteItemOption(position, saved.ArticleId, saved.FabricId, saved.Title,
                    saved.Details, input.IsRecommended, saved.Retail with { Shown = input.Retail.Shown },
                    saved.Wholesale with { Shown = input.Wholesale.Shown }, saved.Large with { Shown = input.Large.Shown },
                    saved.PricesTakenAt)
                .WithFabrics(saved.Fabrics.OrderBy(x => x.Position).Select(x => x.Copy())));
        if (input.ArticleId is null) return Result.Conflict($"«{itemName}»: такого рядка вже немає в КП.");

        var variant = product?.Variants.FirstOrDefault(x => x.ArticleId == input.ArticleId);
        if (variant is null) return Result.Conflict($"«{itemName}»: такого варіанта тканини немає в каталозі.");
        if (!variant.IsActive) return Result.Conflict($"«{itemName}»: варіант «{variant.Title}» вимкнений.");
        if (variant.Prices is not { } prices)
            return Result.Conflict($"«{itemName}», «{variant.Title}»: немає опублікованої ціни ({string.Join(", ", variant.Problems)}).");

        return Result.Success(new QuoteItemOption(position, variant.ArticleId, variant.FabricId, variant.Title,
            variant.Details, input.IsRecommended,
            new QuotePrice(prices.Retail, null, input.Retail.Shown),
            new QuotePrice(prices.Wholesale, null, input.Wholesale.Shown),
            new QuotePrice(prices.Large, null, input.Large.Shown),
            now));
    }

    private static Result<QuoteItemOption> BuildPpeRow(
        int position, QuoteOptionInput input, QuoteItem? previous, QuoteCatalogProduct? product, string itemName,
        QuotePerson actor, DateTimeOffset now)
    {
        var saved = previous?.Options.FirstOrDefault(x => x.Id == input.Id);
        QuoteTierPrices? source = saved is not null
            ? new QuoteTierPrices(saved.Retail.Source ?? 0, saved.Wholesale.Source ?? 0, saved.Large.Source ?? 0)
            : product?.Prices;
        if (source is null)
            return Result.Conflict($"«{itemName}»: немає ціни в каталозі ({string.Join(", ", product?.Problems ?? [])}).");

        var option = new QuoteItemOption(position, null, null, "Ціна", null, false,
            Price(source.Retail, input.Retail), Price(source.Wholesale, input.Wholesale), Price(source.Large, input.Large),
            saved?.PricesTakenAt ?? now);
        return WithManualAudit(option, input, saved, itemName, actor, now);
    }

    /// <summary>An embroidery item keeps its saved price; the editor changes only the description and the ticks.</summary>
    private static Result<QuoteItem> KeepEmbroidery(int position, QuoteItemInput input, QuoteItem? previous)
    {
        if (previous?.EmbroideryCalculationId is null)
            return Result.Invalid(new ValidationError("Вишивку додають кнопкою «Додати в КП» у «Ціна вишивки»."));
        var option = input.Options[0];
        return Result.Success(QuoteEmbroideryItems.Keep(position, previous, input.Description, option.Retail.Shown,
            option.Wholesale.Shown, option.Large.Shown));
    }

    private static Result<QuoteItem> BuildOneOff(
        int position, QuoteItemInput input, QuoteItem? previous, QuotePerson actor, DateTimeOffset now)
    {
        var optionInput = input.Options[0];
        var saved = previous?.Options.FirstOrDefault(x => x.Id == optionInput.Id);
        var name = input.Name!.Trim();
        var option = new QuoteItemOption(0, null, null, "Ціна", null, false,
            new QuotePrice(null, optionInput.Retail.Manual, optionInput.Retail.Shown),
            new QuotePrice(null, optionInput.Wholesale.Manual, optionInput.Wholesale.Shown),
            new QuotePrice(null, optionInput.Large.Manual, optionInput.Large.Shown),
            null);
        var audited = WithManualAudit(option, optionInput, saved, name, actor, now, reasonRequired: false);
        if (!audited.IsSuccess) return audited.Map(_ => default(QuoteItem)!);

        var texts = new QuoteItemTexts(null, name, OneOffSubtitle, input.Description, null, input.PhotoUrl, null,
            input.SupplierUrl);
        return Result.Success(new QuoteItem(position, QuoteItemKind.OneOffPpe, null, texts, [audited.Value]));
    }

    /// <summary>
    /// Keeps who set the manual prices and why while they do not change; a changed manual price records the actor
    /// and needs a reason (R-08).
    /// </summary>
    private static Result<QuoteItemOption> WithManualAudit(
        QuoteItemOption option, QuoteOptionInput input, QuoteItemOption? saved, string itemName, QuotePerson actor,
        DateTimeOffset now, bool reasonRequired = true)
    {
        if (!option.HasManualPrice)
        {
            option.SetManualAudit(null, null, null);
            return Result.Success(option);
        }

        if (reasonRequired && string.IsNullOrWhiteSpace(input.ManualReason))
            return InvalidOption($"«{itemName}»: вкажіть причину ручної ціни.");

        var unchanged = saved is not null && saved.Retail.Manual == option.Retail.Manual &&
            saved.Wholesale.Manual == option.Wholesale.Manual && saved.Large.Manual == option.Large.Manual &&
            saved.ManualReason == input.ManualReason?.Trim();
        if (unchanged)
            option.SetManualAudit(saved!.ManualReason, saved.ManualByUserId is { } id
                ? new QuotePerson(id, saved.ManualByName ?? "")
                : null, saved.ManualAt);
        else
            option.SetManualAudit(input.ManualReason, actor, now);
        return Result.Success(option);
    }

    private static QuotePrice Price(decimal source, QuotePriceInput input) =>
        new(source, input.Manual is { } manual && manual != source ? manual : null, input.Shown);

    private static Result<IReadOnlyList<QuoteItem>> InvalidItems(string message) =>
        Result.Invalid(new ValidationError(message));

    private static Result<QuoteItemOption> InvalidOption(string message) =>
        Result.Invalid(new ValidationError(message));
}
