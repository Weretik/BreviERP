namespace Crm.Application.Features.Embroidery.Shared.Validation;

/// <summary>Codes are enum names exactly (`Hoop`, not `1` or `hoop`).</summary>
internal static class EmbroideryCodes
{
    public static bool Is<TEnum>(string? value) where TEnum : struct, Enum =>
        value is not null && Enum.GetNames<TEnum>().Contains(value, StringComparer.Ordinal);
}
