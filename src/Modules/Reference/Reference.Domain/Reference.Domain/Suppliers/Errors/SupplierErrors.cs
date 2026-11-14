using BuildingBlocks.Domain.Errors;

namespace Reference.Domain.Suppliers.Errors;

public static class SupplierErrors
{
    public static Error IdIsRequired() =>
        new("Reference.Supplier.Id.Required", "Ідентифікатор постачальника є обов'язковим.");

    public static Error NameIsRequired() =>
        new("Reference.Supplier.Name.Required", "Назва постачальника є обов'язковою.");

    public static Error NotesLengthInvalid(int length) =>
        new("Reference.Supplier.Notes.LengthInvalid",
            $"Примітки постачальника не можуть перевищувати 500 символів. Поточна довжина: {length}");

    public static Error KeepinCrmIdIsRequired() =>
        new("Reference.Supplier.KeepinCrmId.Required", "Ідентифікатор KeepinCRM є обов'язковим.");

    public static Error ContactsManagedByKeepinCrm() =>
        new("Reference.Supplier.Contacts.ManagedByKeepinCrm",
            "Контакти цього постачальника оновлюються з KeepinCRM і не редагуються в Brevi.");
}
