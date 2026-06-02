namespace Reference.Domain.Errors;

public static class GarmentPartOperationErrors
{
    public static ReferenceDomainError IdIsRequired() =>
        new("Reference.GarmentPartOperation.Id.Required", "Garment part operation id must be provided");

    public static ReferenceDomainError GarmentPartIdIsRequired() =>
        new("Reference.GarmentPartOperation.GarmentPartId.Required", "Garment part id must be provided");

    public static ReferenceDomainError NameIsRequired() =>
        new("Reference.GarmentPartOperation.Name.Required", "Garment part operation name is required");

    public static ReferenceDomainError MinMustBeNonNegative() =>
        new("Reference.GarmentPartOperation.Min.Invalid", "Garment part operation minutes must be non-negative");
}
