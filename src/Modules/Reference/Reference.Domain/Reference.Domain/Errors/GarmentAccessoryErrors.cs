namespace Reference.Domain.Errors;

public static class GarmentAccessoryErrors
{
    public static ReferenceDomainError IdIsRequired() =>
        new("Reference.GarmentAccessory.Id.Required", "Garment accessory id must be provided");

    public static ReferenceDomainError NameIsRequired() =>
        new("Reference.GarmentAccessory.Name.Required", "Garment accessory name is required");

    public static ReferenceDomainError PriceOutOfRange(decimal min, decimal max) =>
        new("Reference.GarmentAccessory.Price.OutOfRange", $"Garment accessory price amount must be between {min} and {max}");

    public static ReferenceDomainError SupplierIdIsRequired() =>
        new("Reference.GarmentAccessory.SupplierId.Required", "Garment accessory supplier id must be provided");
}
