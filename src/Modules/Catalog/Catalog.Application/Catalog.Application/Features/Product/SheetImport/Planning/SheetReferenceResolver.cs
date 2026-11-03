using Catalog.Application.Contracts.Reference;
using Catalog.Domain.Products.Enums;
using Catalog.Application.Features.Product.Overview.Shared;

namespace Catalog.Application.Features.Product.SheetImport.Planning;

/// <summary>
/// Turns sheet codes and names into Reference IDs. Codes are compared trimmed and upper-case; supplier and section
/// names trimmed and ignoring case. Only the known sheet typos in <see cref="SupplierAliases"/> are corrected;
/// there is no fuzzy matching.
/// </summary>
public sealed class SheetReferenceResolver
{
    /// <summary>Supplier names as written in the sheet → the Reference name.</summary>
    public static readonly IReadOnlyDictionary<string, string> SupplierAliases =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["3C-Макркет"] = "3C-Маркет" };

    private readonly Dictionary<string, int> _fabrics;
    private readonly Dictionary<string, int> _accessories;
    private readonly Dictionary<string, int> _operations;
    private readonly Dictionary<string, int> _suppliers;
    private readonly Dictionary<(string Type, string Name), int> _sections;

    public SheetReferenceResolver(ProductReferenceCodes references)
    {
        _fabrics = ByCode(references.Fabrics);
        _accessories = ByCode(references.Accessories);
        _operations = ByCode(references.Operations);
        _suppliers = references.Suppliers.OrderBy(x => x.Id)
            .GroupBy(x => x.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First().Id, StringComparer.OrdinalIgnoreCase);
        _sections = references.Sections.OrderBy(x => x.Id)
            .GroupBy(x => (x.Type, x.Name.Trim().ToUpperInvariant()))
            .ToDictionary(x => x.Key, x => x.First().Id);
    }

    public int? Fabric(string? code) => Find(_fabrics, code);

    public int? Accessory(string? code) => Find(_accessories, code);

    public int? Operation(string? code) => Find(_operations, code);

    public int? Section(ProductType type, string? name)
        => string.IsNullOrWhiteSpace(name)
            ? null
            : _sections.TryGetValue((ProductOverviewTypes.Name(type), name.Trim().ToUpperInvariant()), out var id) ? id : null;

    public int? Supplier(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var key = name.Trim();
        if (_suppliers.TryGetValue(key, out var id)) return id;
        return SupplierAliases.TryGetValue(key, out var alias) && _suppliers.TryGetValue(alias, out var aliased)
            ? aliased
            : null;
    }

    public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    private static int? Find(Dictionary<string, int> items, string? code)
        => string.IsNullOrWhiteSpace(code) ? null : items.TryGetValue(NormalizeCode(code), out var id) ? id : null;

    private static Dictionary<string, int> ByCode(IEnumerable<ReferenceCodeItem> items)
        => items.OrderBy(x => x.Id).GroupBy(x => NormalizeCode(x.Code)).ToDictionary(x => x.Key, x => x.First().Id);
}
