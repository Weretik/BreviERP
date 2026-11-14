using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;
using Crm.Application.Abstractions.Quotes.Products;

namespace UnitTests.Crm.Quotes;

internal sealed class FakeQuoteCatalog : IQuoteCatalogReader
{
    public Dictionary<int, QuoteCatalogProduct> Products { get; } = [];

    public static QuoteCatalogProduct Model(int id = 10, bool active = true, params QuoteCatalogVariant[] variants) =>
        new(id, QuoteItemKind.Sewing, "MOD-1", "Костюм «Імпульс»", "Літо", active, "https://brevi.com.ua/impuls",
            "Літній костюм", null, null, null, [], variants);

    public static QuoteCatalogVariant Variant(long articleId = 100, decimal? retail = 900, bool active = true) =>
        new(articleId, $"SKU-{articleId}", active, 5, "Саржа", "65% поліестер", "120 грн/м · Тканини",
            retail is { } r ? new QuoteTierPrices(r, r - 50, r - 100) : null, retail is null ? ["NO_LABOR"] : []);

    public static QuoteCatalogProduct Ppe(int id = 20, decimal? retail = 300) =>
        new(id, QuoteItemKind.Ppe, "PPE-1", "Рукавиці", null, true, null, "Захисні рукавиці", null, null,
            retail is { } r ? new QuoteTierPrices(r, r - 20, r - 40) : null, retail is null ? ["NO_OFFER"] : [], []);

    public FakeQuoteCatalog With(QuoteCatalogProduct product)
    {
        Products[product.ProductId] = product;
        return this;
    }

    public Task<QuoteCatalogPage> SearchAsync(QuoteItemKind kind, string? search, string? season, int page, int pageSize,
        CancellationToken cancellationToken) =>
        Task.FromResult(new QuoteCatalogPage(Products.Values.Where(x => x.Kind == kind).ToArray(), page, pageSize,
            Products.Count));

    public Task<IReadOnlyDictionary<int, QuoteCatalogProduct>> GetAsync(IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<int, QuoteCatalogProduct>>(
            Products.Where(x => productIds.Contains(x.Key)).ToDictionary());
}

internal sealed class FakeSewingCalculator : IQuoteSewingCalculator
{
    /// <summary>Price per metre of each fabric; a fabric missing here has no price.</summary>
    public Dictionary<int, decimal> FabricPrices { get; } = new() { [5] = 200m, [6] = 300m };

    public int Calls { get; private set; }

    public Task<QuoteSewingCalculation> CalculateAsync(
        QuoteSewingComposition composition, CancellationToken cancellationToken)
    {
        Calls++;
        var minutes = composition.Lines.Where(x => x.Kind == QuoteItemLineKind.Operation)
            .Sum(x => x.Quantity * (x.ManualMinutes ?? 10m));
        var lines = composition.Lines.Select(x => new QuoteSewingLineInfo(x.Kind, x.ReferenceId,
            x.ReferenceId == 404 ? null : $"{x.Kind} {x.ReferenceId}", x.Kind == QuoteItemLineKind.Operation ? "хв" : "шт.",
            10m, true)).ToArray();
        var variants = composition.Variants.Select(variant =>
        {
            var infos = variant.Fabrics.Select(x => new QuoteSewingFabricInfo(x.FabricId, x.Meters, $"Тканина {x.FabricId}",
                "65% бавовна", FabricPrices.TryGetValue(x.FabricId, out var p) ? p : null)).ToArray();
            if (infos.Any(x => x.PricePerMeter is null))
                return new QuoteSewingVariantPrice(infos, minutes, null, null, null, ["немає ціни: тканина"]);
            var cost = infos.Sum(x => x.Meters * x.PricePerMeter!.Value) + minutes * 5m;
            return new QuoteSewingVariantPrice(infos, minutes, minutes * 5m, cost,
                new QuoteTierPrices(cost * 2, cost * 1.8m, cost * 1.5m), []);
        }).ToArray();
        return Task.FromResult(new QuoteSewingCalculation(lines, variants));
    }
}

/// <summary>A model with one accessory and one 10-minute operation.</summary>
internal sealed class FakeModelCompositionReader : IQuoteModelCompositionReader
{
    public Task<QuoteModelComposition?> ReadAsync(int productId, CancellationToken cancellationToken) =>
        Task.FromResult<QuoteModelComposition?>(productId != 10 ? null : new QuoteModelComposition(10,
            [new QuoteModelFabric(100, 5, "Саржа", 1.2m, true)],
            [
                new QuoteModelLine(QuoteItemLineKind.Accessory, 30, null, "Гудзик", "шт.", 2m, null),
                new QuoteModelLine(QuoteItemLineKind.Operation, 40, null, "Шов", "хв", 1m, null)
            ]));
}
