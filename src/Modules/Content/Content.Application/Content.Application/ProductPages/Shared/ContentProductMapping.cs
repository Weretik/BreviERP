using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Domain.ProductPages;

namespace Content.Application.ProductPages.Shared;

internal static class ContentProductMapping
{
    public static ContentProductRow Row(ContentCatalogProduct product, ProductPage? page, int categoryCount)
    {
        var uk = page?.TextIn(ProductPageRules.Ukrainian);
        var ru = page?.TextIn(ProductPageRules.Russian);
        return new ContentProductRow(product.Id, product.Type, product.Code, product.Name, product.SectionId,
            product.SectionName, product.IsActive, product.MainPhotoUrl, product.Prices is not null, page is not null,
            page?.ShowOnSite ?? false, page is { ShowOnSite: true } && product.IsActive, page?.Slug,
            uk is { HasText: true }, ru is { HasText: true }, uk is { HasSeo: true } && ru is { HasSeo: true },
            page?.UpdatedAtUtc, categoryCount);
    }

    public static ContentProductTextResponse Text(ProductPageText? text) => text is null
        ? new ContentProductTextResponse(null, null, null, [], [], [])
        : new ContentProductTextResponse(text.Name, text.SeoTitle, text.SeoDescription, text.Summary, text.Description,
            text.Specs);

    public static ContentPriceTiersResponse? Prices(ContentPriceTiers? prices) =>
        prices is null ? null : new ContentPriceTiersResponse(prices.Retail, prices.Wholesale, prices.LargeWholesale);

    public static ContentProductResponse Detail(ContentCatalogProduct product, ProductPage? page, string offeredSlug,
        IReadOnlyList<int> categoryIds) =>
        new(Row(product, page, categoryIds.Count), page?.Revision ?? 0, page?.Slug ?? offeredSlug, page?.UpdatedBy,
            Text(page?.TextIn(ProductPageRules.Ukrainian)), Text(page?.TextIn(ProductPageRules.Russian)),
            Prices(product.Prices),
            product.Variants.Select(x => new ContentProductVariantResponse(x.ArticleId, x.SaleSku, x.IsActive,
                x.FabricName, x.FabricDetails, Prices(x.Prices))).ToArray(), categoryIds);
}
