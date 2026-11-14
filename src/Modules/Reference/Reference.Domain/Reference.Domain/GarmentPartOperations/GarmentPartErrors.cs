using BuildingBlocks.Domain.Errors;

namespace Reference.Domain.GarmentPartOperations;

public static class GarmentPartErrors
{
    public static Error NameIsRequired() =>
        new("Reference.GarmentPart.Name.Required", "Вкажіть назву деталі виробу.");
}
