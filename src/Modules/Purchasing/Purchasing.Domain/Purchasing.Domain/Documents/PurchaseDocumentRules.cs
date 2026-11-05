using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Purchasing.Domain.Documents;

/// <summary>
/// Reads a document from JSON (an AI answer or a file uploaded by hand) and checks it. Money is compared in kopecks:
/// the positions must add up to the total (plus VAT when prices are without VAT). A line whose sum is up to 30 % below
/// quantity × price is a discount and is accepted when the totals add up; a larger sum is a misreading. Unknown fields
/// are dropped.
/// </summary>
public static partial class PurchaseDocumentRules
{
    public const int MaxItems = 200;
    public const int TextMaxLength = 300;
    public const int NameMaxLength = 1_000;

    [GeneratedRegex(@"^\d{1,12}(\.\d{1,4})?$")]
    private static partial Regex Decimal4();

    [GeneratedRegex(@"^\d{1,12}(\.\d{1,2})?$")]
    private static partial Regex Money();

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$")]
    private static partial Regex IsoDate();

    public static string VatName(PurchaseVatMode mode) => mode switch
    {
        PurchaseVatMode.WithVat => "with_vat",
        _ => "without_vat",
    };

    /// <summary>
    /// Fields of the AI answer and of a manual file. <paramref name="sourceUid"/> and <paramref name="originalFileLink"/>
    /// are set by the server for an AI answer; a manual file must contain the same values.
    /// </summary>
    public static (PurchaseDocument? Document, IReadOnlyList<string> Problems) Read(JsonElement json, string sourceUid,
        string originalFileLink, bool identityInJson)
    {
        var problems = new List<string>();
        if (json.ValueKind != JsonValueKind.Object) return (null, ["Документ має бути об'єктом JSON."]);

        if (identityInJson)
        {
            if (Text(json, "sourceUID") != sourceUid) problems.Add($"sourceUID має бути {sourceUid}.");
            if (Text(json, "originalFileLink") != originalFileLink) problems.Add("originalFileLink не збігається з посиланням цього файлу.");
        }

        var documentType = Required(json, "documentType", "Вид документа", TextMaxLength, problems);
        var number = Required(json, "documentNumber", "Номер рахунку", 100, problems);
        var supplier = Required(json, "supplierName", "Постачальник", TextMaxLength, problems);

        var date = Text(json, "documentDate")?.Trim();
        if (date is null || !IsoDate().IsMatch(date)
            || !DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            problems.Add("Дата рахунку не знайдена або не у форматі РРРР-ММ-ДД.");

        var vat = Text(json, "vat") switch
        {
            "with_vat" => PurchaseVatMode.WithVat,
            "without_vat" => PurchaseVatMode.WithoutVat,
            _ => (PurchaseVatMode?)null,
        };
        if (vat is null) problems.Add("vat має бути with_vat або without_vat.");
        var pricesIncludeVat = vat == PurchaseVatMode.WithVat
            && json.TryGetProperty("pricesIncludeVat", out var include) && include.ValueKind == JsonValueKind.True;

        var items = new List<PurchaseDocumentItem>();
        var gaps = new List<LineGap>();
        if (!json.TryGetProperty("items", out var array) || array.ValueKind != JsonValueKind.Array || array.GetArrayLength() == 0)
            problems.Add("Немає жодної позиції.");
        else if (array.GetArrayLength() > MaxItems)
            problems.Add($"Позицій більше {MaxItems}.");
        else
        {
            var position = 0;
            foreach (var item in array.EnumerateArray())
            {
                position++;
                var row = ReadItem(item, position, problems, gaps);
                if (row is not null) items.Add(row);
            }
        }

        var total = Text(json, "totalPayable")?.Trim();
        if (total is null || !Money().IsMatch(total)) problems.Add("Сума до оплати не знайдена або не число з крапкою (до 2 знаків).");

        var vatAmount = Text(json, "vatAmount")?.Trim();
        if (vat == PurchaseVatMode.WithVat)
        {
            if (vatAmount is null || !Money().IsMatch(vatAmount)) problems.Add("Для рахунку з ПДВ потрібна сума ПДВ (число з крапкою).");
        }
        else if (vatAmount is not null && Kopecks(vatAmount) is not (null or 0))
            problems.Add("Рахунок без ПДВ не може мати суму ПДВ.");
        else vatAmount = null;

        var totalsAddUp = problems.Count == 0 && CheckArithmetic(items, vat!.Value, pricesIncludeVat, vatAmount, total!, problems);
        foreach (var gap in gaps.Where(x => !(totalsAddUp && x.Discount)))
            problems.Add($"Позиція {gap.Position}: кількість × ціна = {gap.Expected.ToString("0.00", CultureInfo.InvariantCulture)}, а сума {gap.Sum}.");
        if (problems.Count > 0) return (null, problems);

        return (new PurchaseDocument(sourceUid, originalFileLink, documentType!, number!, date!, supplier!, vat!.Value,
            pricesIncludeVat, items, vatAmount, total!), problems);
    }

    /// <summary>A discount of up to 30 %: a lower sum than that is more likely a misread quantity or price.</summary>
    private const decimal MinDiscountedShare = 0.7m;

    /// <summary>A line whose sum differs from quantity × price; a sum lower by up to 30 % is a discount.</summary>
    private sealed record LineGap(int Position, decimal Expected, string Sum, bool Discount);

    private static PurchaseDocumentItem? ReadItem(JsonElement item, int position, List<string> problems, List<LineGap> gaps)
    {
        var prefix = $"Позиція {position}";
        if (item.ValueKind != JsonValueKind.Object)
        {
            problems.Add($"{prefix}: має бути об'єктом.");
            return null;
        }

        var before = problems.Count;
        var name = Required(item, "name", $"{prefix}: назва", NameMaxLength, problems);
        var quantity = Text(item, "quantity")?.Trim();
        if (quantity is null || !Decimal4().IsMatch(quantity) || Units(quantity) == 0)
            problems.Add($"{prefix}: кількість має бути додатним числом з крапкою (до 4 знаків).");
        var unit = Text(item, "unit")?.Trim();
        if (unit is { Length: 0 }) unit = null;
        if (unit is { Length: > 30 }) problems.Add($"{prefix}: одиниця довша за 30 символів.");
        var price = Text(item, "price")?.Trim();
        if (price is null || !Decimal4().IsMatch(price)) problems.Add($"{prefix}: ціна має бути числом з крапкою (до 4 знаків).");
        var sum = Text(item, "sum")?.Trim();
        if (sum is null || !Money().IsMatch(sum)) problems.Add($"{prefix}: сума має бути числом з крапкою (до 2 знаків).");
        if (problems.Count > before) return null;

        // Rounded unit prices are common, so only a clear difference counts: more than 1 грн and 1 %.
        var expected = decimal.Parse(quantity!, CultureInfo.InvariantCulture) * decimal.Parse(price!, CultureInfo.InvariantCulture);
        var actual = decimal.Parse(sum!, CultureInfo.InvariantCulture);
        var gap = Math.Abs(expected - actual);
        if (gap > 1m && gap > actual * 0.01m)
            gaps.Add(new LineGap(position, expected, sum!, actual < expected && actual >= expected * MinDiscountedShare));

        return new PurchaseDocumentItem(position, name!, quantity!, unit, price!, sum!);
    }

    /// <summary>True when the positions add up to the total; otherwise adds the problem.</summary>
    private static bool CheckArithmetic(IReadOnlyList<PurchaseDocumentItem> items, PurchaseVatMode vat, bool pricesIncludeVat,
        string? vatAmount, string total, List<string> problems)
    {
        var lines = items.Sum(x => Kopecks(x.Sum)!.Value);
        var expected = Kopecks(total)!.Value;
        var withVat = vat == PurchaseVatMode.WithVat && !pricesIncludeVat ? lines + Kopecks(vatAmount!)!.Value : lines;
        if (withVat == expected) return true;

        problems.Add(vat == PurchaseVatMode.WithVat && !pricesIncludeVat
            ? $"Сума позицій {Format(lines)} + ПДВ {vatAmount} = {Format(withVat)}, а до оплати {total}."
            : $"Сума позицій {Format(lines)}, а до оплати {total}.");
        return false;
    }

    /// <summary>Exact kopecks of a money string; null when it is not one.</summary>
    public static long? Kopecks(string value)
    {
        if (!Money().IsMatch(value)) return null;
        var parts = value.Split('.');
        var fraction = parts.Length == 2 ? parts[1].PadRight(2, '0') : "00";
        return long.Parse(parts[0], CultureInfo.InvariantCulture) * 100 + long.Parse(fraction, CultureInfo.InvariantCulture);
    }

    private static decimal Units(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

    private static string Format(long kopecks) =>
        (kopecks / 100m).ToString("0.00", CultureInfo.InvariantCulture);

    private static string? Required(JsonElement json, string property, string label, int maxLength, List<string> problems)
    {
        var value = Text(json, property)?.Trim();
        if (string.IsNullOrEmpty(value)) problems.Add($"{label}: не знайдено.");
        else if (value.Length > maxLength) problems.Add($"{label}: довше за {maxLength} символів.");
        else return value;
        return null;
    }

    /// <summary>A string property; numbers are accepted as their JSON text, so «8000» and 8000 read the same.</summary>
    private static string? Text(JsonElement json, string property) =>
        json.TryGetProperty(property, out var value)
            ? value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.GetRawText(),
                _ => null,
            }
            : null;
}
