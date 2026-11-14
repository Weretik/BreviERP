using Reference.Domain.GarmentPartOperations;

namespace Reference.Application.Features.GarmentPartOperation.Shared;

internal static class GarmentPartOperationDetailsFactory
{
    public static GarmentPartOperationDetails From(IGarmentPartOperationFields fields, GarmentPartId garmentPartId) =>
        new(fields.Code, garmentPartId, fields.Element, fields.Variant, fields.Name, fields.Min,
            fields.MinutesPerProduct, fields.IsActive, fields.Notes);
}
