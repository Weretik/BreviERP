using BuildingBlocks.Domain.Errors;

namespace Crm.Domain.Embroidery.Settings;

/// <summary>Business rule errors of the embroidery settings and saved calculations.</summary>
public static class EmbroiderySettingsErrors
{
    public static Error Invalid(string field, string message) =>
        Error.Validation("crm.embroidery_settings_invalid", message, field);

    public static Error CategoryNotInResult() =>
        Error.Validation("crm.embroidery_category_unknown", "Оберіть категорію ціни.", "category");
}
