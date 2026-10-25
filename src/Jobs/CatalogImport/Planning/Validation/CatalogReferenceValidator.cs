using CatalogImport.Source;

namespace CatalogImport.Planning;

public static class CatalogReferenceValidator
{
    public static void Validate(CatalogImportPlan plan, CatalogSourceDiagnostics diagnostics)
    {
        var source = plan.Snapshot;
        foreach (var row in source.Rows("Тканини").Except(CatalogSourceReferences.Fabrics(source)))
        {
            diagnostics.Require(!plan.Materials.Any(material => material.Text(6) == row.Text(0)), row,
                "an incomplete fabric is referenced by composition");
            diagnostics.Warnings.Add($"{row.Key}: empty inactive fabric placeholder retained only in source provenance.");
        }
        foreach (var row in CatalogSourceReferences.Fabrics(source).Concat(source.Rows("Фурнітура")))
        {
            var priceColumn = row.Sheet == "Тканини" ? 5 : 3;
            diagnostics.Require(row.Decimal(priceColumn) is >= 0, row, "material cost must be nonnegative");
            if (row.Text(2).Length == 0)
                diagnostics.Warnings.Add($"{row.Key}: {row.Text(0)} has no supplier; use existing unspecified-supplier reference.");
        }
        foreach (var row in source.Rows("Операції"))
        {
            diagnostics.Require(row.Decimal(5) is null or > 0, row, "operation norm must be positive or absent");
            if (row.Decimal(5) is null)
            {
                diagnostics.Require(plan.Operations.Where(op => op.Text(5) == row.Text(0)).All(op => op.Decimal(8) is > 0),
                    row, "missing norm requires manual time on every usage");
                diagnostics.Warnings.Add($"{row.Key}: absent base norm; legacy reference uses zero, all imported uses require manual minutes. Original null is preserved in provenance.");
            }
        }
    }
}

