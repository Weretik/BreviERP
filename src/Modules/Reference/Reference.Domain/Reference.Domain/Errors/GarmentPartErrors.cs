namespace Reference.Domain.Errors;

public static class GarmentPartErrors
{
    public static ReferenceDomainError IdIsRequired() =>
        new("Reference.GarmentPart.Id.Required", "Garment part id must be provided");

    public static ReferenceDomainError NameIsRequired() =>
        new("Reference.GarmentPart.Name.Required", "Garment part name is required");
}
