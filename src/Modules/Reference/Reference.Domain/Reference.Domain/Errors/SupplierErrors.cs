namespace Reference.Domain.Errors;

public static class SupplierErrors
{
    public static ReferenceDomainError IdIsRequired() =>
        new("Reference.Supplier.Id.Required", "Ідентифікатор постачальника є обов'язковим.");

    public static ReferenceDomainError NameIsRequired() =>
        new("Reference.Supplier.Name.Required", "Назва постачальника є обов'язковою.");

    public static ReferenceDomainError NotesLengthInvalid(int length) =>
        new("Reference.Supplier.Notes.LengthInvalid",
            $"Довжина приміток постачальника не може перевищувати 500 символів. Поточна довжина: {length}");
}
