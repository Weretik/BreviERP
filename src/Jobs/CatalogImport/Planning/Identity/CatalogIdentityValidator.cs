using CatalogImport.Source;

namespace CatalogImport.Planning;

public static class CatalogIdentityValidator
{
    public static CatalogIdentityKeys Validate(
        CatalogImportPlan plan, IReadOnlyList<WorkbookRow> priceRows, CatalogSourceDiagnostics diagnostics)
    {
        var keys = new CatalogIdentityKeys(Unique(plan.Models, diagnostics), Unique(plan.Ppe, diagnostics),
            [], [], [], []);
        Unique(priceRows, diagnostics);
        keys = keys with
        {
            Articles = Unique(plan.Articles, diagnostics),
            Fabrics = Unique(CatalogSourceReferences.Fabrics(plan.Snapshot), diagnostics),
            Accessories = Unique(plan.Snapshot.Rows("Фурнітура"), diagnostics),
            Operations = Unique(plan.Snapshot.Rows("Операції"), diagnostics)
        };
        Unique(plan.Snapshot.Rows("Постачальники"), diagnostics);
        if (plan.Models.Count == 0 || plan.Articles.Count == 0 || plan.Ppe.Count == 0)
            diagnostics.Error("A full catalogue snapshot requires models, sewing articles and PPE.");
        if (priceRows.Count != plan.Articles.Count + plan.Ppe.Count
            || !priceRows.Where(row => row.Text(5) == "ЗІЗ").Select(row => row.Text(0)).ToHashSet().SetEquals(keys.Ppe))
            diagnostics.Error("Price-list and PPE identities do not reconcile.");
        if (keys.Articles.Overlaps(keys.Ppe))
            diagnostics.Error("A sale SKU occurs in both Sewing and PPE.");
        foreach (var article in plan.Articles)
            diagnostics.Require(keys.Models.Contains(CatalogSourceIdentity.ModelCode(article)), article, "unknown model");
        foreach (var model in plan.Models)
        {
            diagnostics.Require(plan.Articles.Any(row => CatalogSourceIdentity.ModelCode(row) == model.Text(0)), model, "no sale articles");
            diagnostics.Require(plan.Operations.Any(row => row.Text(1) == model.Text(0)), model, "no operations");
            diagnostics.Require(plan.Materials.Any(row => row.Text(0) == model.Text(0)), model, "no materials");
        }
        return keys;
    }

    public static void ValidatePpeOffers(CatalogImportPlan plan, CatalogSourceDiagnostics diagnostics)
    {
        foreach (var row in plan.Ppe)
        {
            diagnostics.Require(row.Text(5).Length != 0, row, "supplier is required");
            diagnostics.Require(row.Decimal(7) is null or > 0, row, "supplier price must be positive or absent");
            if (row.Decimal(7) is null)
                diagnostics.Warnings.Add($"{row.Key}: SKU {row.Text(0)} has no supplier price; retained as null.");
        }
    }

    private static HashSet<string> Unique(IReadOnlyList<WorkbookRow> rows, CatalogSourceDiagnostics diagnostics)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in rows)
            diagnostics.Require(keys.Add(row.Text(0)), row, "duplicate source key");
        return keys;
    }
}

