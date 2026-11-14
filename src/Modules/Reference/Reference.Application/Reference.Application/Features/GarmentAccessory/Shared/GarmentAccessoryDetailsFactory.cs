using Reference.Application.Features.GarmentAccessory.Shared.DTOs;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.Suppliers;
using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.Shared;

internal static class GarmentAccessoryDetailsFactory
{
    public static GarmentAccessoryDetails From(IGarmentAccessoryFields fields, SupplierId supplierId) => new(
        fields.Code, fields.Name, supplierId, fields.Unit ?? GarmentAccessoryEntity.DefaultUnit,
        fields.IsActive, fields.Comment);
}
