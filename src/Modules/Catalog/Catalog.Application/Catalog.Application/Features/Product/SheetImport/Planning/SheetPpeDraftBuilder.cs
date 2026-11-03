using Catalog.Application.Features.Product.SheetImport.DTOs;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.SheetImport.Planning;

/// <summary>
/// Resolves one «ЗІЗ» row into a draft. An unknown section or supplier is a problem; an unknown pricing rule, a
/// missing supplier price or link are warnings: the product is imported without them.
/// </summary>
public sealed class SheetPpeDraftBuilder(
    SheetReferenceResolver references,
    IReadOnlySet<string> knownRuleCodes,
    SheetImportIssueLog log)
{
    private const string Scope = "ppe";

    /// <returns>Null when the row has a problem.</returns>
    public SheetProductDraft? Build(SheetPpeRow row)
    {
        var before = log.Problems.Count;
        var code = row.Code.Trim();
        void Problem(string message) => log.Problem(Scope, row.RowNumber, code, message);
        void Warning(string message) => log.Warning(Scope, row.RowNumber, code, message);

        var sectionId = references.Section(ProductType.Ppe, row.Section);
        if (sectionId is null)
            Problem(string.IsNullOrWhiteSpace(row.Section)
                ? "Немає розділу."
                : $"Розділ «{row.Section.Trim()}» не знайдено серед розділів ЗІЗ довідника «Типи і розділи».");

        var supplierId = references.Supplier(row.SupplierName);
        if (supplierId is null)
            Problem(string.IsNullOrWhiteSpace(row.SupplierName)
                ? "Немає постачальника."
                : $"Постачальника «{row.SupplierName.Trim()}» не знайдено в довіднику «Постачальники».");

        if (row.SupplierPrice is <= 0)
            Problem("Ціна постачальника має бути більша за 0.");
        else if (row.SupplierPrice is { } price && decimal.Round(price, 6) != price)
            Problem("Ціна постачальника — не більше 6 знаків після коми.");
        if (row.InsightWholesale is < 0 || row.InsightRetail is < 0)
            Problem("Ціни Insight не можуть бути від'ємні.");

        var ruleCode = Trim(row.RuleCode);
        if (ruleCode is not null && !knownRuleCodes.Contains(ruleCode))
        {
            Warning($"Правило ціноутворення «{ruleCode}» не знайдено: товар імпортується без правила, ціни не рахуються, доки правило не вибрано.");
            ruleCode = null;
        }
        if (row.SupplierPrice is null)
            Warning("Немає ціни постачальника: версію ціни не створено, ціни не рахуються.");
        if (string.IsNullOrWhiteSpace(row.SupplierUrl))
            Warning("Немає посилання на товар постачальника.");

        if (log.Problems.Count > before)
            return null;
        return new SheetProductDraft(row.RowNumber, ProductType.Ppe, code, row.Name.Trim(),
            new ProductOverviewDetails(sectionId, null, row.IsActive,
                Trim(row.CurrentSiteUrl), Trim(row.QuoteDescription)),
            new PpeSupplierDetails(SupplierReferenceId.Create(supplierId!.Value), Trim(row.SupplierSku),
                Trim(row.SupplierUrl), row.SupplierPrice, row.AllowManualPrice, ruleCode, Trim(row.Unit)),
            null);
    }

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
