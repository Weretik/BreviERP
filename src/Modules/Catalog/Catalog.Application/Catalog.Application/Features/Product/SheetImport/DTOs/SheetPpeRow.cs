namespace Catalog.Application.Features.Product.SheetImport.DTOs;

/// <summary>One row of the Calculator tab «ЗІЗ».</summary>
/// <param name="Code">Site article (sale SKU).</param>
/// <param name="SupplierName">Reference supplier name.</param>
/// <param name="SupplierPrice">Null means the product gets no price version.</param>
/// <param name="RuleCode">PPE pricing rule code (the rule name in the sheet).</param>
/// <param name="InsightWholesale">Insight XML wholesale price, when the sheet has it.</param>
/// <param name="InsightRetail">Insight XML retail price, when the sheet has it.</param>
public sealed record SheetPpeRow(
    int RowNumber,
    string Code,
    string Name,
    string? Section,
    string? Unit,
    string? CurrentSiteUrl,
    string? SupplierName,
    string? SupplierUrl,
    string? SupplierSku,
    decimal? SupplierPrice,
    bool IsActive,
    bool AllowManualPrice,
    string? RuleCode,
    string? QuoteDescription,
    decimal? InsightWholesale,
    decimal? InsightRetail);
