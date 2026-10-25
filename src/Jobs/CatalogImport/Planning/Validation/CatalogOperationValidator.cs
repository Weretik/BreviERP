namespace CatalogImport.Planning;

public static class CatalogOperationValidator
{
    public static void Validate(CatalogImportPlan plan, CatalogIdentityKeys keys, CatalogSourceDiagnostics diagnostics)
    {
        foreach (var row in plan.Operations)
        {
            diagnostics.Require(keys.Models.Contains(row.Text(1)), row, "unknown model");
            diagnostics.Require(keys.Operations.Contains(row.Text(5))
                || (row.Text(5).Length == 0 && row.Decimal(8) is > 0 && row.Text(11).Length != 0),
                row, "unknown operation");
            if (row.Text(5).Length == 0)
                diagnostics.Warnings.Add($"{row.Key}: manual operation retained under a row-based technical key.");
            diagnostics.Require(row.Decimal(7) is > 0, row, "quantity must be positive");
            diagnostics.Require(row.Decimal(8) is null or > 0, row, "manual minutes must be positive");
        }
    }
}

