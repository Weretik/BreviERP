using BuildingBlocks.Domain.Errors;

namespace Reference.Domain.GarmentAccessories.Errors;

public static class FabricErrors
{
    public static Error IdIsRequired() =>
        new("Reference.Fabric.Id.Required", "Не вказано тканину.");

    public static Error NameIsRequired() =>
        new("Reference.Fabric.Name.Required", "Вкажіть назву тканини.");

    public static Error PriceOutOfRange(decimal min, decimal max) =>
        new("Reference.Fabric.Price.OutOfRange", $"Ціна тканини — від {min} до {max}.");

    public static Error ProviderIdIsRequired() =>
        new("Reference.Fabric.ProviderId.Required", "Оберіть постачальника тканини.");

    public static Error TextTooLong(int maxLength) =>
        new("Reference.Fabric.Text.TooLong", $"Текст тканини — не більше {maxLength} символів.");
}
