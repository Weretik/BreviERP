using System.Globalization;
using System.Text.RegularExpressions;
using Ardalis.Result;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Catalog.Application.Abstractions.Pricing;

namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Files;

/// <summary>
/// Reads the СЕМТО PDF price list from its text layer: the article is a word in the left column, the price
/// ("651,72" or "1 633,86") stands before «грн.» in the same row. An article listed twice with different prices
/// is returned with a problem, like the client's script.
/// </summary>
internal static partial class CemtoPdfReader
{
    private const double ArticleColumnRight = 130;
    private const double PriceColumnLeft = 180;
    private const double PriceColumnRight = 300;
    private const double RowTolerance = 15;

    public static Result<IReadOnlyList<SupplierFilePrice>> Read(IReadOnlyList<SupplierPriceFile> files)
    {
        var found = new Dictionary<string, (string Article, List<decimal> Prices)>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            try
            {
                using var buffer = new MemoryStream();
                using (var content = CompressedUpload.Open(file.Content)) content.CopyTo(buffer);
                using var document = PdfDocument.Open(buffer.ToArray());
                foreach (var page in document.GetPages())
                    foreach (var (article, price) in ReadPage(page))
                    {
                        var key = article.ToUpperInvariant();
                        if (!found.TryGetValue(key, out var entry)) found[key] = entry = (article, []);
                        if (!entry.Prices.Contains(price)) entry.Prices.Add(price);
                    }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                return Invalid($"Файл «{file.Name}» не вдалося прочитати як PDF-прайс СЕМТО.");
            }
        }

        if (found.Count == 0)
            return Invalid("У PDF не знайдено артикулів із цінами. Потрібен прайс СЕМТО з текстом, не скан.");

        return Result.Success<IReadOnlyList<SupplierFilePrice>>(found.Values
            .Select(x => x.Prices.Count == 1
                ? new SupplierFilePrice(x.Article, null, null, x.Prices[0], null, null, null)
                : new SupplierFilePrice(x.Article, null, null, x.Prices.Min(), null, null,
                    "У PDF різні ціни для цього артикулу. Ціну не змінено."))
            .ToList());
    }

    private static IEnumerable<(string Article, decimal Price)> ReadPage(Page page)
    {
        var words = page.GetWords().ToList();
        var left = words.Where(w => w.BoundingBox.Left < ArticleColumnRight).ToList();
        var articles = left
            .Where(w => ArticlePattern().IsMatch(w.Text))
            .Select(w => (Text: ArticleText(w, left), w.BoundingBox.Bottom))
            .ToList();

        // A price may stand alone or after its thousands ("1 633,86"); «грн.» is not always printed.
        var priceWords = words
            .Where(w => w.BoundingBox.Left >= PriceColumnLeft && w.BoundingBox.Left < PriceColumnRight
                        && DigitsPattern().IsMatch(w.Text))
            .OrderBy(w => w.BoundingBox.Left)
            .ToList();
        foreach (var row in priceWords.GroupBy(w => Math.Round(w.BoundingBox.Bottom / 3)))
        {
            var text = string.Concat(row.Select(w => w.Text));
            if (!PricePattern().IsMatch(text)) continue;
            var bottom = row.First().BoundingBox.Bottom;
            var article = articles
                .Select(x => (x.Text, Distance: Math.Abs(x.Bottom - bottom)))
                .Where(x => x.Distance <= RowTolerance)
                .OrderBy(x => x.Distance)
                .Select(x => x.Text)
                .FirstOrDefault();
            if (article is not null)
                yield return (article, decimal.Parse(text.Replace(',', '.'), CultureInfo.InvariantCulture));
        }
    }

    /// <summary>An article cell can wrap: "9054" with "winter" on the next line is the article "9054 winter".</summary>
    private static string ArticleText(Word anchor, IReadOnlyList<Word> left)
    {
        var below = left
            .Where(w => w != anchor && w.BoundingBox.Bottom < anchor.BoundingBox.Bottom
                        && anchor.BoundingBox.Bottom - w.BoundingBox.Bottom <= RowTolerance
                        && !ArticlePattern().IsMatch(w.Text) && WordPattern().IsMatch(w.Text))
            .OrderByDescending(w => w.BoundingBox.Bottom)
            .Select(w => w.Text);
        return string.Join(' ', below.Prepend(anchor.Text));
    }

    private static Result<IReadOnlyList<SupplierFilePrice>> Invalid(string message)
        => Result<IReadOnlyList<SupplierFilePrice>>.Invalid(new ValidationError("files", message));

    [GeneratedRegex(@"^(?=.*\d)[0-9A-Za-z]{3,}$")]
    private static partial Regex ArticlePattern();

    [GeneratedRegex(@"^\d{1,6},\d{2}$")]
    private static partial Regex PricePattern();

    [GeneratedRegex(@"^[\d,]+$")]
    private static partial Regex DigitsPattern();

    [GeneratedRegex(@"^[0-9A-Za-z]+$")]
    private static partial Regex WordPattern();
}
