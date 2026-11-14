
using BuildingBlocks.Domain.Results;

namespace Reference.Domain.GarmentAccessories;

public static class GarmentAccessoryErrors
{
    public static Error NameIsRequired() =>
        new("Reference.GarmentAccessory.Name.Required", "Вкажіть назву фурнітури.", Field: "name");

    public static Error PriceOutOfRange(decimal min, decimal max) =>
        new("Reference.GarmentAccessory.Price.OutOfRange", $"Ціна фурнітури — від {min} до {max}.", Field: "price");

    public static Error SupplierIdIsRequired() =>
        new("Reference.GarmentAccessory.SupplierId.Required", "Оберіть постачальника фурнітури.", Field: "supplierId");

    public static Error TextTooLong(int maxLength) =>
        new("Reference.GarmentAccessory.Text.TooLong", $"Текст фурнітури — не більше {maxLength} символів.");

    public static Error UnitInvalid(string allowed) =>
        new("Reference.GarmentAccessory.Unit.Invalid", $"Одиниця фурнітури — одна з: {allowed}.", Field: "unit");
}
