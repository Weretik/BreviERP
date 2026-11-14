
using BuildingBlocks.Domain.Results;

namespace Reference.Domain.Suppliers;

public static class SupplierErrors
{
    public static Error NameIsRequired() =>
        new("Reference.Supplier.Name.Required", "Назва постачальника є обов'язковою.", Field: "name");

    public static Error NotesLengthInvalid(int length) =>
        new("Reference.Supplier.Notes.LengthInvalid",
            $"Примітки постачальника не можуть перевищувати 500 символів. Поточна довжина: {length}", Field: "notes");

    public static Error KeepinCrmIdIsRequired() =>
        new("Reference.Supplier.KeepinCrmId.Required", "Ідентифікатор KeepinCRM є обов'язковим.");

    public static Error ContactsManagedByKeepinCrm() =>
        new("Reference.Supplier.Contacts.ManagedByKeepinCrm",
            "Контакти цього постачальника оновлюються з KeepinCRM і не редагуються в Brevi.");
}
