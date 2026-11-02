using Reference.Domain.Errors;

namespace Reference.Domain.GarmentPartOperations.Errors;

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

    public static ReferenceDomainError MinOutOfRange(decimal max) =>
        new("Reference.GarmentPartOperation.Min.OutOfRange", $"Garment part operation minutes must be from 0 to {max}");

    public static ReferenceDomainError TextTooLong(int maxLength) =>
        new("Reference.GarmentPartOperation.Text.TooLong", $"Garment part operation text field must not exceed {maxLength} characters");
}
