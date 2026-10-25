using CatalogImport.Source;

namespace CatalogImport.Planning;

public static class CatalogImportPlanner
{
    public static CatalogImportPlan Build(WorkbookSnapshot source)
    {
        var diagnostics = new CatalogSourceDiagnostics();
        var models = source.Rows("Товари");
        var priceRows = source.Rows("Прайс на сайт");
        var plan = new CatalogImportPlan(source, models,
            priceRows.Where(row => row.Text(5) == "Швейна продукція").ToArray(),
            source.Rows("ЗІЗ"), source.Rows("Комплектація виробів"),
            source.Rows("Трудовитрати виробів", 1), diagnostics.Warnings);
        var keys = CatalogIdentityValidator.Validate(plan, priceRows, diagnostics);
        CatalogMaterialValidator.Validate(plan, keys, diagnostics);
        CatalogOperationValidator.Validate(plan, keys, diagnostics);
        CatalogIdentityValidator.ValidatePpeOffers(plan, diagnostics);
        CatalogReferenceValidator.Validate(plan, diagnostics);
        diagnostics.ThrowIfInvalid();
        return plan;
    }
}
