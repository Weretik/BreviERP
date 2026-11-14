using BuildingBlocks.Domain.Errors;

namespace Reference.Domain.GarmentPartOperations.Errors;

public static class GarmentPartErrors
{
    public static Error IdIsRequired() =>
        new("Reference.GarmentPart.Id.Required", "Не вказано деталь виробу.");

    public static Error NameIsRequired() =>
        new("Reference.GarmentPart.Name.Required", "Вкажіть назву деталі виробу.");
}
