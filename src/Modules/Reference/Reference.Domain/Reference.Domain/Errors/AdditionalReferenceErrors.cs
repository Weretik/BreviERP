namespace Reference.Domain.Errors;

public static class AdditionalReferenceErrors
{
    public static ReferenceDomainError IdIsRequired() =>
        new("Reference.AdditionalReference.Id.Required",
            "Additional reference id must be provided");

    public static ReferenceDomainError NameIsRequired() =>
        new("Reference.AdditionalReference.Name.Required",
            "Additional reference name is required");

    public static ReferenceDomainError ValueMustBeNonNegative() =>
        new("Reference.AdditionalReference.Value.Invalid",
            "Additional reference value must be non-negative");

    public static ReferenceDomainError UnitIsRequired() =>
        new("Reference.AdditionalReference.Unit.Required",
            "Additional reference unit is required");
}
