using System.Globalization;
using System.Xml;
using Ardalis.Result;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices.Sources;

namespace Catalog.Infrastructure.Integrations.PpeSupplierFiles;

/// <summary>
/// Reads Insight's three YML price lists (dealer, wholesale, retail in any order), like the client's script:
/// every size of a product shares its link, so one link must have one price per file; the three prices sorted
/// give dealer &lt; wholesale &lt; retail. Each product is returned once by link and once by article.
/// </summary>
internal static class InsightYmlReader
{
    private const string MissingInFiles = "Товар знайдено не в усіх трьох XML.";
    private const string SizeConflict = "У розмірів в одному XML різні ціни. Ціну не змінено.";

    public static async Task<Result<IReadOnlyList<SupplierFilePrice>>> ReadAsync(
        IReadOnlyList<SupplierPriceFile> files, CancellationToken cancellationToken)
    {
        var parsed = new List<Dictionary<string, Entry>>();
        foreach (var file in files)
        {
            try
            {
                await using var content = CompressedUpload.Open(file.Content);
                var entries = await ReadFileAsync(content, cancellationToken);
                if (entries.Count == 0)
                    return Invalid($"У файлі «{file.Name}» немає товарів. Потрібні XML-прайси Insight.");
                parsed.Add(entries);
            }
            catch (Exception exception) when (exception is XmlException or InvalidDataException)
            {
                return Invalid($"Файл «{file.Name}» не є XML-прайсом Insight.");
            }
        }

        var keys = parsed.SelectMany(x => x.Keys).Distinct(StringComparer.Ordinal);
        var prices = new List<SupplierFilePrice>();
        foreach (var key in keys)
        {
            var records = parsed.Select(x => x.GetValueOrDefault(key)).OfType<Entry>().ToList();
            var meta = records[0];
            var url = key.StartsWith("url:", StringComparison.Ordinal) ? meta.Url : null;
            var sku = url is null ? meta.Sku : null;
            string? problem = records.Count != parsed.Count ? MissingInFiles
                : records.Any(x => x.Conflict) ? SizeConflict
                : null;
            var sorted = records.Select(x => x.Price).Order().ToList();
            prices.Add(problem is null && sorted.Count == 3
                ? new SupplierFilePrice(sku, url, meta.Name, sorted[0], sorted[1], sorted[2], null)
                : new SupplierFilePrice(sku, url, meta.Name, sorted[0], null, null, problem));
        }

        return Result.Success<IReadOnlyList<SupplierFilePrice>>(prices);
    }

    private static async Task<Dictionary<string, Entry>> ReadFileAsync(Stream content, CancellationToken cancellationToken)
    {
        var settings = new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };
        using var reader = XmlReader.Create(content, settings);
        var entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
        while (await reader.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (reader.NodeType != XmlNodeType.Element || reader.Name != "offer") continue;
            var offer = await ReadOfferAsync(reader.ReadSubtree());
            if (offer.Price is not > 0) continue;
            Add(entries, SupplierKeys.Url(offer.Url) is { } url ? "url:" + url : null, offer);
            Add(entries, SupplierKeys.Sku(offer.Sku) is { } sku ? "sku:" + sku : null, offer);
        }

        return entries;
    }

    private static void Add(Dictionary<string, Entry> entries, string? key, Offer offer)
    {
        if (key is null) return;
        if (!entries.TryGetValue(key, out var entry))
            entries[key] = new Entry(offer.Sku, offer.Url, offer.Name, offer.Price!.Value, false);
        else if (entry.Price != offer.Price)
            entries[key] = entry with { Conflict = true };
    }

    private static async Task<Offer> ReadOfferAsync(XmlReader offer)
    {
        using (offer)
        {
            string? url = null, vendorCode = null, article = null, name = null, price = null;
            // Reading an element's content already moves to the next node, so read again only after other nodes.
            var moved = await offer.ReadAsync();
            while (moved)
            {
                if (offer.NodeType != XmlNodeType.Element || offer.Depth != 1)
                {
                    moved = await offer.ReadAsync();
                    continue;
                }

                switch (offer.Name)
                {
                    case "url": url = await offer.ReadElementContentAsStringAsync(); break;
                    case "vendorCode": vendorCode = await offer.ReadElementContentAsStringAsync(); break;
                    case "article": article = await offer.ReadElementContentAsStringAsync(); break;
                    case "name": name = await offer.ReadElementContentAsStringAsync(); break;
                    case "price": price = await offer.ReadElementContentAsStringAsync(); break;
                    default:
                        await offer.SkipAsync();
                        break;
                }

                moved = !offer.EOF;
            }

            var sku = string.IsNullOrWhiteSpace(vendorCode) ? article : vendorCode;
            return new Offer(Trim(url), Trim(sku), Trim(name), ParsePrice(price));
        }
    }

    private static decimal? ParsePrice(string? value)
        => decimal.TryParse(value?.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var price)
            ? price
            : null;

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Result<IReadOnlyList<SupplierFilePrice>> Invalid(string message)
        => Result<IReadOnlyList<SupplierFilePrice>>.Invalid(new ValidationError("files", message));

    private sealed record Offer(string? Url, string? Sku, string? Name, decimal? Price);

    private sealed record Entry(string? Sku, string? Url, string? Name, decimal Price, bool Conflict);
}
