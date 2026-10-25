using CatalogImport.Source;

namespace CatalogImport.Planning;

public static class CatalogMaterialValidator
{
    public static void Validate(CatalogImportPlan plan, CatalogIdentityKeys keys, CatalogSourceDiagnostics diagnostics)
    {
        var materialKeys = new HashSet<string>();
        foreach (var row in plan.Materials)
        {
            diagnostics.Require(keys.Models.Contains(row.Text(0)), row, "unknown model");
            if (row.Text(1).Length != 0)
                diagnostics.Require(plan.Articles.Any(a => a.Text(0) == row.Text(1)
                    && CatalogSourceIdentity.ModelCode(a) == row.Text(0)), row, "article does not belong to model");
            var isFabric = row.Text(6).StartsWith("FAB-", StringComparison.Ordinal);
            diagnostics.Require((isFabric ? keys.Fabrics : keys.Accessories).Contains(row.Text(6)), row, "unknown material");
            diagnostics.Require(row.Decimal(8) is > 0, row, "quantity must be positive");
            diagnostics.Require(isFabric ? row.Text(9) == "м" : row.Text(9) is "м" or "шт." or "послуга",
                row, "unsupported material unit");
            diagnostics.Require(materialKeys.Add($"{row.Text(0)}|{row.Text(1)}|{row.Text(6)}"), row, "duplicate material scope");
            if (row.Text(9) == "послуга")
                diagnostics.Warnings.Add($"{row.Key}: service {row.Text(6)} retained as OtherCost; pricing remains blocked.");
        }
    }
}

