using Ardalis.Result;
using System.Collections.Concurrent;
using Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Import;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;
using Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.Cabinets;

namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online;

/// <summary>
/// Reads АВ-Центр and Перчатка-центр prices from each product's page, 3C-Маркет prices from its six published
/// lists, and Insight and Трєле dealer prices from their cabinets. Pages load a few at a time and each link once,
/// even when sizes of a product share it.
/// </summary>
internal sealed class PpeSupplierOnlinePriceReader(SupplierPageClient pages, SupplierCabinetPriceReader cabinets)
    : IPpeSupplierOnlinePriceReader
{
    private const int ParallelPages = 4;

    private sealed record Website(string Host, string Name, string UserAgent, Func<string, decimal?> FindPrice);

    private static readonly IReadOnlyDictionary<string, Website> Websites = new Dictionary<string, Website>
    {
        [PpePriceSources.AvCenter] = new("avcentr.com.ua", "АВ-Центру",
            "Mozilla/5.0 (compatible; BreviPriceUpdater/1.0)", AvCenterPagePrice.Find),
        [PpePriceSources.PerchatkaCentr] = new("perchatka-centr.com.ua", "Перчатка-центру",
            "Mozilla/5.0", PerchatkaCentrPagePrice.Find),
    };

    private static readonly IReadOnlyDictionary<string, ISupplierCabinet> Cabinets = new Dictionary<string, ISupplierCabinet>
    {
        [PpePriceSources.Insight] = new InsightCabinet(),
        [PpePriceSources.Trele] = new TreleCabinet(),
    };

    public async Task<Result<IReadOnlyList<SupplierFilePrice>>> ReadAsync(
        PpePriceSource source, IReadOnlyList<PpePriceImportProduct> products, CancellationToken cancellationToken)
    {
        if (source.Key == PpePriceSources.ThreeCMarket) return await ReadThreeCAsync(cancellationToken);
        if (Websites.TryGetValue(source.Key, out var website))
            return Result.Success(await ReadPagesAsync(website, products, cancellationToken));
        if (Cabinets.TryGetValue(source.Key, out var cabinet))
            return await cabinets.ReadAsync(cabinet, products, cancellationToken);
        return Result.Invalid(new ValidationError("key", $"{source.Title} не оновлюється з сайту, таблиць чи кабінету."));
    }

    private async Task<IReadOnlyList<SupplierFilePrice>> ReadPagesAsync(
        Website website, IReadOnlyList<PpePriceImportProduct> products, CancellationToken cancellationToken)
    {
        var links = SupplierLinks.Distinct(products);

        var prices = new ConcurrentBag<SupplierFilePrice>();
        await Parallel.ForEachAsync(links,
            new ParallelOptions { MaxDegreeOfParallelism = ParallelPages, CancellationToken = cancellationToken },
            async (url, token) => prices.Add(await ReadPageAsync(website, url, token)));
        return prices.ToList();
    }

    private async Task<SupplierFilePrice> ReadPageAsync(Website website, string url, CancellationToken cancellationToken)
    {
        SupplierFilePrice Problem(string message) => new(null, url, null, 0m, null, null, message);

        if (!SupplierLinks.IsOnSite(url, website.Host))
            return Problem($"Посилання постачальника веде не на сайт {website.Name} ({website.Host}).");
        var page = await pages.GetAsync(url, website.UserAgent, cancellationToken);
        if (page.Text is null) return Problem(page.Problem!);
        return website.FindPrice(page.Text) is { } price
            ? new SupplierFilePrice(null, url, null, price, null, null, null)
            : Problem("На сторінці постачальника не знайдено ціну.");
    }

    private async Task<Result<IReadOnlyList<SupplierFilePrice>>> ReadThreeCAsync(CancellationToken cancellationToken)
    {
        var lists = await Task.WhenAll(ThreeCMarketPriceLists.All.Select(async list =>
            (list.Title, Page: await pages.GetAsync(list.Url, "Mozilla/5.0", cancellationToken))));

        var failed = lists.Where(list => list.Page.Text is null).ToList();
        if (failed.Count > 0)
            return Result.Invalid(failed.Select(list => new ValidationError("source",
                $"Прайс 3C «{list.Title}» не завантажився: {list.Page.Problem} Ціни не змінено, спробуйте пізніше.")).ToList());

        return Result.Success(ThreeCMarketPriceLists.Merge(
            lists.Select(list => (list.Title, ThreeCMarketPriceLists.Parse(list.Page.Text!)))));
    }
}
