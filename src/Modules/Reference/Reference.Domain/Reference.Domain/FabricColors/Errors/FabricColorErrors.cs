using Reference.Domain.Errors;

namespace Reference.Domain.FabricColors.Errors;

public static class FabricColorErrors
{
    public static ReferenceDomainError IdIsRequired() =>
        new("Reference.FabricColor.Id.Required", "Не вказано ідентифікатор кольору.");

    public static ReferenceDomainError NameIsRequired() =>
        new("Reference.FabricColor.Name.Required", "Назва кольору є обов'язковою.");

    public static ReferenceDomainError NameIsTooLong(int maxLength) =>
        new("Reference.FabricColor.Name.TooLong", $"Назва кольору: не більше {maxLength} символів.");
}
