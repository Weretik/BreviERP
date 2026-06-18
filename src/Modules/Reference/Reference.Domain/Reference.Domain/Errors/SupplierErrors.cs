namespace Reference.Domain.Errors;

public static class SupplierErrors
{
    public static ReferenceDomainError IdIsRequired() =>
        new("Reference.Supplier.Id.Required", "Supplier id must be provided");

    public static ReferenceDomainError NameIsRequired() =>
        new("Reference.Supplier.Name.Required", "Supplier name is required");

    public static ReferenceDomainError NotesLengthInvalid(int length) =>
        new("Reference.Supplier.Notes.LengthInvalid",
            $"Supplier notes length must be at most 500 characters. Actual:{length}");
}
