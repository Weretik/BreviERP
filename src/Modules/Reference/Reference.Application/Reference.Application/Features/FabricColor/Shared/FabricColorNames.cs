using FabricColorEntity = Reference.Domain.FabricColors.Entities.FabricColor;

namespace Reference.Application.Features.FabricColor.Shared;

/// <summary>
/// Two names are the same colour when they differ only in case or spaces, as «Склад тканин» keys a stock line by
/// fabric and colour.
/// </summary>
public static class FabricColorNames
{
    public const string Duplicate = "Такий колір уже є в довіднику.";

    public static string Key(string name) =>
        string.Concat(name.Where(c => !char.IsWhiteSpace(c))).ToLowerInvariant();

    public static bool Taken(IEnumerable<FabricColorEntity> colors, string name, int? exceptId = null)
    {
        var key = Key(name);
        return colors.Any(x => x.Id.Value != exceptId && Key(x.Name) == key);
    }

    public static Domain.FabricColors.ValueObjects.FabricColorDetails Details(FabricColorFields fields) =>
        new(fields.Name, fields.IsActive, fields.SortOrder);
}
