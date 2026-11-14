using BuildingBlocks.Domain.Errors;

namespace Reference.Domain.GarmentAccessories.Errors;

public static class GarmentAccessoryErrors
{
    public static Error IdIsRequired() =>
        new("Reference.GarmentAccessory.Id.Required", "Не вказано фурнітуру.");

    public static Error NameIsRequired() =>
        new("Reference.GarmentAccessory.Name.Required", "Вкажіть назву фурнітури.");

    public static Error PriceOutOfRange(decimal min, decimal max) =>
        new("Reference.GarmentAccessory.Price.OutOfRange", $"Ціна фурнітури — від {min} до {max}.");

    public static Error SupplierIdIsRequired() =>
        new("Reference.GarmentAccessory.SupplierId.Required", "Оберіть постачальника фурнітури.");

    public static Error TextTooLong(int maxLength) =>
        new("Reference.GarmentAccessory.Text.TooLong", $"Текст фурнітури — не більше {maxLength} символів.");

    public static Error UnitInvalid(string allowed) =>
        new("Reference.GarmentAccessory.Unit.Invalid", $"Одиниця фурнітури — одна з: {allowed}.");
}
