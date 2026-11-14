using BuildingBlocks.Domain.Errors;

using Reference.Domain.GarmentPartOperations;

namespace Reference.Domain.GarmentParts;

public static class GarmentPartErrors
{
    public static Error NameIsRequired() =>
        new("Reference.GarmentPart.Name.Required", "Вкажіть назву деталі виробу.");
}
