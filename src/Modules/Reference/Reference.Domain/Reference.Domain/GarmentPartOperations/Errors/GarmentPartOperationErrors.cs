using BuildingBlocks.Domain.Errors;

namespace Reference.Domain.GarmentPartOperations.Errors;

public static class GarmentPartOperationErrors
{
    public static Error GarmentPartIdIsRequired() =>
        new("Reference.GarmentPartOperation.GarmentPartId.Required", "Не вказано деталь виробу.");

    public static Error NameIsRequired() =>
        new("Reference.GarmentPartOperation.Name.Required", "Вкажіть назву операції.");

    public static Error MinMustBeNonNegative() =>
        new("Reference.GarmentPartOperation.Min.Invalid", "Хвилини операції не можуть бути від'ємними.");

    public static Error MinOutOfRange(decimal max) =>
        new("Reference.GarmentPartOperation.Min.OutOfRange", $"Хвилини операції — від 0 до {max}.");

    public static Error TextTooLong(int maxLength) =>
        new("Reference.GarmentPartOperation.Text.TooLong", $"Текст операції — не більше {maxLength} символів.");
}
