using Catalog.Domain.Products;

namespace Catalog.Application.Features.Product.Commands.ImportSheetProducts;

/// <summary>
/// Checks the body before matching: every row needs a code and a name; codes of a type, product names (both types
/// share one name column) and sale SKUs (sewing articles and PPE site articles) must not repeat.
/// </summary>
public static class SheetBodyDuplicates
{
    /// <returns>The rows that must not be imported, as (type, position in its list).</returns>
    public static HashSet<(ProductType Type, int Index)> Find(ImportSheetProductsCommand command, SheetImportIssueLog log)
    {
        var rows = command.Sewing.Select((x, i) => new Row(ProductType.Sewing, i, x.RowNumber, Code(ProductType.Sewing, x.Code), x.Name?.Trim(),
                x.Articles.Select(a => a.SaleSku?.Trim()).OfType<string>().Where(a => a.Length > 0).ToList()))
            .Concat(command.Ppe.Select((x, i) => new Row(ProductType.Ppe, i, x.RowNumber, Code(ProductType.Ppe, x.Code), x.Name?.Trim(),
                string.IsNullOrWhiteSpace(x.Code) ? [] : [x.Code.Trim()])))
            .ToList();
        var flagged = new HashSet<(ProductType, int)>();

        foreach (var row in rows)
        {
            if (string.IsNullOrEmpty(row.Code)) Flag(row, "Немає коду товару.");
            if (string.IsNullOrEmpty(row.Name)) Flag(row, "Немає назви товару.");
        }

        foreach (var group in rows.Where(x => !string.IsNullOrEmpty(x.Code)).GroupBy(x => (x.Type, x.Code)).Where(x => x.Count() > 1))
            foreach (var row in group.Skip(1))
                Flag(row, $"Код {row.Code} уже є в рядку {group.First().RowNumber}.", group.First());

        foreach (var group in rows.Where(x => !string.IsNullOrEmpty(x.Name)).GroupBy(x => x.Name, StringComparer.Ordinal).Where(x => x.Count() > 1))
            foreach (var row in group.Skip(1))
                Flag(row, $"Назва «{row.Name}» уже є в рядку {group.First().RowNumber} ({Label(group.First())}). " +
                    "Назви товарів мають бути унікальні: додайте до назви уточнення (колір, розмір, модель).", group.First());

        var skus = rows.SelectMany(row => row.Skus.Select(sku => (Row: row, Sku: sku)))
            .GroupBy(x => x.Sku, StringComparer.Ordinal).Where(x => x.Select(y => y.Row).Distinct().Count() > 1);
        foreach (var group in skus)
        {
            var first = group.First().Row;
            foreach (var (row, sku) in group.Where(x => x.Row != first))
                Flag(row, $"Артикул на сайті {sku} уже використано в рядку {first.RowNumber} ({Label(first)}).", first);
        }

        return flagged;

        void Flag(Row row, string message, Row? other = null)
        {
            log.Problem(SheetImportPlan.Scope(row.Type), row.RowNumber, row.Code, message);
            flagged.Add((row.Type, row.Index));
            if (other is not null) flagged.Add((other.Type, other.Index));
        }
    }

    private static string? Code(ProductType type, string? code)
        => string.IsNullOrWhiteSpace(code) ? null
            : type == ProductType.Sewing ? SheetReferenceResolver.NormalizeCode(code) : code.Trim();

    private static string Label(Row row) => (row.Type == ProductType.Sewing ? "модель " : "ЗІЗ ") + row.Code;

    private sealed record Row(ProductType Type, int Index, int RowNumber, string? Code, string? Name, IReadOnlyList<string> Skus);
}
