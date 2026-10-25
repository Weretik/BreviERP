using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Product.GetAdminDetail.DTOs;
using Catalog.Domain.Products.Productivity;

namespace Catalog.Application.Contracts.Admin.Sewing;

internal static class SewingAdminDetailMapper
{
    public static SewingAdminDetail Map(SewingReadModel sewing, ProductReferenceData references)
    {
        var minutes = sewing.OperationIds.Select(id => references.GarmentPartOperations.GetValueOrDefault(id)?.Minutes ?? 0m).ToList();
        return new(sewing.MetersPerProduct,
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
            SewingProductivity.CalculatePiecesPerShift(minutes), null);
    }
}
