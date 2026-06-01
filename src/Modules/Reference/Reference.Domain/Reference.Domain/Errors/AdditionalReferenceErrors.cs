namespace Reference.Domain.Errors;

public static class AdditionalReferenceErrors
{
    public static ReferenceDomainError IdIsRequired() =>
        new("Reference.AdditionalReference.Id.Required",
            "Additional reference id must be provided");

    public static ReferenceDomainError NameIsRequired() =>
        new("Reference.AdditionalReference.Name.Required",
            "Additional reference name is required");

    public static ReferenceDomainError KeyIsRequired() =>
        new("Reference.AdditionalReference.Key.Required",
            "Additional reference key is required");

    public static ReferenceDomainError ValueMustBeNonNegative() =>
        new("Reference.AdditionalReference.Value.Invalid",
            "Additional reference value must be non-negative");

    public static ReferenceDomainError UnitIsRequired() =>
        new("Reference.AdditionalReference.Unit.Required",
            "Additional reference unit is required");

    public static ReferenceDomainError UnitIsInvalid() =>
        new("Reference.AdditionalReference.Unit.Invalid",
            "Additional reference unit must be one of: шт., грн., %");
}
