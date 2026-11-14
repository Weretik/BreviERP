using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices.Sources;
using Catalog.Application.Abstractions.Pricing.PpeSupplierPrices;
using Catalog.Application.Abstractions.Reference.Items;

namespace Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices;

/// <summary>
/// The «Постачальники» row a source updates: the one chosen in «Імпорт цін», or else the supplier whose name matches
/// the source (the names come from KeepinCRM, for example «Інсайт» or «3C-Макркет»).
/// </summary>
public static class PpePriceSourceSupplier
{
    public static ReferenceItem? Resolve(
        PpePriceSource source, PpePriceSourceSettings? settings, IReadOnlyDictionary<int, ReferenceItem> suppliers)
    {
        if (settings?.SupplierId is { } chosen) return suppliers.GetValueOrDefault(chosen);

        var aliases = source.SupplierAliases.Select(SupplierKeys.Name).ToHashSet(StringComparer.Ordinal);
        return suppliers.Values
            .Where(supplier => aliases.Contains(SupplierKeys.Name(supplier.Name)))
            .OrderBy(supplier => supplier.Id)
            .FirstOrDefault();
    }
}
