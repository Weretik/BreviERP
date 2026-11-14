
using Reference.Domain.GarmentAccessories;
using BuildingBlocks.Domain.Results;

namespace Reference.Domain.Fabrics;

public static class FabricErrors
{
    public static Error NameIsRequired() =>
        new("Reference.Fabric.Name.Required", "Вкажіть назву тканини.", Field: "name");

    public static Error PriceOutOfRange(decimal min, decimal max) =>
        new("Reference.Fabric.Price.OutOfRange", $"Ціна тканини — від {min} до {max}.", Field: "price");

    public static Error ProviderIdIsRequired() =>
        new("Reference.Fabric.ProviderId.Required", "Оберіть постачальника тканини.", Field: "providerId");

    public static Error TextTooLong(int maxLength) =>
        new("Reference.Fabric.Text.TooLong", $"Текст тканини — не більше {maxLength} символів.");
}
