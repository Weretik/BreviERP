using Catalog.Application.Contracts.Reference;
using Catalog.Domain.Products.Pricing;
using Catalog.Domain.Products.Productivity;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Application.Contracts.Admin.Ppe;
using Catalog.Application.Contracts.Admin.Reference;
using Catalog.Application.Contracts.Admin.Sewing;
using Catalog.Application.Features.Product.GetAdminDetail.DTOs;

namespace Catalog.Application.Contracts.Admin.Product;

public static class ProductAdminDetailMapper
{
    public static IReadOnlyCollection<string> SewingPricingReferenceKeys { get; } =
    [
        "sr_zp_shvei", "work_day", "coefficient_seamstress_award", "coefficient_factor",
        "coefficient_master", "coefficient_foreman", "monthly_expenses", "count_shvei",
        "profit_10", "profit_10_40", "profit_40"
    ];

    public static ProductAdminDetail Map(
        ProductAdminDetailReadModel product,
        ProductReferenceData references,
        IReadOnlyDictionary<int, CategoryAdminDetail> categories,
        IReadOnlyDictionary<int, string> mediaUrls) => new(
        product.Id,
        product.Name,
        product.RuName,
        product.Slug,
        product.Type,
        product.DescriptionUk,
        product.DescriptionRu,
        product.CategoryIds.Select(id => categories.GetValueOrDefault(id)).OfType<CategoryAdminDetail>().ToList(),
        product.Photos.OrderBy(x => x.SortOrder)
            .Where(x => mediaUrls.ContainsKey(x.MediaFileId))
            .Select(x => new ProductPhotoDetail(x.MediaFileId, mediaUrls[x.MediaFileId], x.Alt, x.IsVisible, x.IsMain, x.SortOrder)).ToList(),
        product.InformationBlocks.OrderBy(x => x.SortOrder).Select(x => new InformationBlockAdminDetail(x.TitleUk, x.TitleRu, x.TextUk, x.TextRu, x.SortOrder)).ToList(),
        product.CharacteristicTables.OrderBy(x => x.SortOrder).Select(table => new CharacteristicTableAdminDetail(
            table.TitleUk, table.TitleRu, table.SortOrder, table.Rows.OrderBy(row => row.SortOrder)
                .Select(row => new CharacteristicRowAdminDetail(row.LabelUk, row.LabelRu, row.ValueUk, row.ValueRu, row.SortOrder)).ToList())).ToList(),
        product.Sewing is null ? null : MapSewing(product.Sewing, references),
        product.Ppe is null ? null : MapPpe(product.Ppe, references),
        product.CreatedAtUtc,
        product.UpdatedAtUtc);

    private static SewingAdminDetail MapSewing(SewingReadModel sewing, ProductReferenceData references)
    {
        var minutes = sewing.OperationIds.Select(id => references.GarmentPartOperations.GetValueOrDefault(id)?.Minutes ?? 0m).ToList();
        var prices = CalculateSewingPrices(sewing, references);
        return new SewingAdminDetail(
            sewing.MetersPerProduct,
            sewing.Fabrics.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.SortOrder)
                .Select(x => new { Fabric = x, Reference = references.Fabrics.GetValueOrDefault(x.FabricId) })
                .Where(x => x.Reference is not null)
                .Select(x => new FabricAdminDetail(x.Fabric.FabricId, x.Reference!.Name, x.Reference.Price, x.Fabric.IsPrimary, x.Fabric.SortOrder)).ToList(),
            sewing.Accessories.OrderBy(x => x.SortOrder)
                .Select(x => new { Accessory = x, Reference = references.GarmentAccessories.GetValueOrDefault(x.GarmentAccessoryId) })
                .Where(x => x.Reference is not null)
                .Select(x => new AccessoryAdminDetail(x.Accessory.GarmentAccessoryId, x.Reference!.Name, x.Reference.Price, x.Accessory.Quantity, x.Accessory.SortOrder)).ToList(),
            sewing.OperationIds.Select(id => references.GarmentPartOperations.GetValueOrDefault(id)).OfType<OperationReferenceItem>()
                .Select(x => new OperationAdminDetail(x.Id, x.Name, x.Minutes)).ToList(),
            SewingProductivity.CalculatePiecesPerShift(minutes),
            prices);
    }

    private static PpeAdminDetail? MapPpe(PpeReadModel ppe, ProductReferenceData references)
    {
        if (!references.Suppliers.TryGetValue(ppe.SupplierId, out var supplier)) return null;
        var retailPrice = CalculatePpeRetailPrice(ppe, references);
        var wholesalePrice = CalculatePpeWholesalePrice(ppe, references);
        if (!retailPrice.HasValue || !wholesalePrice.HasValue) return null;
        return new PpeAdminDetail(
            new NamedReferenceAdminDetail(supplier.Id, supplier.Name),
            ppe.BasePrice,
            MapPercent(ppe.RetailSource, ppe.RetailAdditionalReferenceId, ppe.RetailCustomPercent, references),
            MapPercent(ppe.WholesaleSource, ppe.WholesaleAdditionalReferenceId, ppe.WholesaleCustomPercent, references),
            retailPrice.Value,
            wholesalePrice.Value);
    }

    public static decimal? CalculatePpeRetailPrice(PpeReadModel ppe, ProductReferenceData references)
        => CalculatePpePrice(ppe.BasePrice, ppe.RetailSource, ppe.RetailAdditionalReferenceId, ppe.RetailCustomPercent, references);

    public static decimal? CalculatePpeWholesalePrice(PpeReadModel ppe, ProductReferenceData references)
        => CalculatePpePrice(ppe.BasePrice, ppe.WholesaleSource, ppe.WholesaleAdditionalReferenceId, ppe.WholesaleCustomPercent, references);

    private static PpePercentAdminDetail MapPercent(PricePercentSource source, int? referenceId, decimal? customPercent, ProductReferenceData references)
    {
        var reference = referenceId is { } id && references.AdditionalReferences.TryGetValue(id, out var item)
            ? new AdditionalReferenceAdminDetail(item.Id, item.Name, item.Key, item.Value, item.Unit)
            : null;
        return new PpePercentAdminDetail(
            source == PricePercentSource.Reference ? PpePercentSource.Reference : PpePercentSource.Custom,
            reference,
            customPercent);
    }

    public static SewingPrices? CalculateSewingPrices(SewingReadModel sewing, ProductReferenceData data)
    {
        if (!TryValues(data, out var values) || sewing.Fabrics.Any(x => !data.Fabrics.ContainsKey(x.FabricId)) || sewing.Accessories.Any(x => !data.GarmentAccessories.ContainsKey(x.GarmentAccessoryId))) return null;
        var accessories = sewing.Accessories.Sum(x => x.Quantity * data.GarmentAccessories.GetValueOrDefault(x.GarmentAccessoryId)?.Price ?? 0m);
        var result = SewingPricing.Calculate(new SewingPricingInput(sewing.MetersPerProduct, sewing.Fabrics.Select(x => new SewingFabricCost(x.FabricId, data.Fabrics[x.FabricId].Price)).ToList(), accessories, sewing.OperationIds.Select(id => data.GarmentPartOperations.GetValueOrDefault(id)?.Minutes ?? 0m).ToList(), new SewingWorkshopCosts(values["sr_zp_shvei"], values["work_day"], values["coefficient_seamstress_award"], values["coefficient_factor"], values["coefficient_master"], values["coefficient_foreman"], values["monthly_expenses"], values["count_shvei"]), new SewingProfitMargins(values["profit_10"], values["profit_10_40"], values["profit_40"])));
        if (result is null) return null;
        var byFabric = result.ByFabric.Select(x => new SewingFabricPrices(x.FabricId, x.Price1To10, x.Price11To39, x.Price40Plus)).ToList();
        SewingPriceRange Range(Func<SewingFabricPrices, decimal> price) { var min = byFabric.MinBy(price)!; var max = byFabric.MaxBy(price)!; return new(price(min), min.FabricId, price(max), max.FabricId); }
        return new SewingPrices(byFabric, new SewingPriceRanges(Range(x => x.Price1To10), Range(x => x.Price11To39), Range(x => x.Price40Plus)));
    }

    private static bool TryValues(ProductReferenceData data, out Dictionary<string, decimal> values)
    {
        values = data.AdditionalReferences.Values.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
        return SewingPricingReferenceKeys.All(values.ContainsKey) && values["work_day"] > 0 && values["coefficient_factor"] > 0 && values["coefficient_master"] > 0 && values["coefficient_foreman"] > 0 && values["count_shvei"] > 0;
    }

    private static decimal? CalculatePpePrice(
        decimal basePrice,
        PricePercentSource source,
        int? additionalReferenceId,
        decimal? customPercent,
        ProductReferenceData references)
    {
        var percent = source == PricePercentSource.Custom
            ? customPercent
            : additionalReferenceId is { } id && references.AdditionalReferences.TryGetValue(id, out var reference)
                ? reference.Value
                : null;

        return percent.HasValue ? basePrice * (1 + percent.Value / 100) : null;
    }
}
