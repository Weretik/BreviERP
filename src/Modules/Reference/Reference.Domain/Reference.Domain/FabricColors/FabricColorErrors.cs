
using BuildingBlocks.Domain.Results;

namespace Reference.Domain.FabricColors;

public static class FabricColorErrors
{
    public static Error NameIsRequired() =>
        new("Reference.FabricColor.Name.Required", "Назва кольору є обов'язковою.");

    public static Error NameIsTooLong(int maxLength) =>
        new("Reference.FabricColor.Name.TooLong", $"Назва кольору: не більше {maxLength} символів.");
}
