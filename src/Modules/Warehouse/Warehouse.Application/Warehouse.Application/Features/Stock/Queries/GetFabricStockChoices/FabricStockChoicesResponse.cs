namespace Warehouse.Application.Features.Stock.Queries.GetFabricStockChoices;

/// <summary>What the stock forms pick from, so the workshop does not need access to «Довідники».</summary>
/// <param name="Suppliers">Names of active «Постачальники» and of the suppliers of «Тканини»; the form may type another.</param>
/// <param name="Colors">Active colours of «Кольори» in list order.</param>
public sealed record FabricStockChoicesResponse(
    IReadOnlyList<FabricStockFabricChoice> Fabrics,
    IReadOnlyList<string> Suppliers,
    IReadOnlyList<string> Colors);
