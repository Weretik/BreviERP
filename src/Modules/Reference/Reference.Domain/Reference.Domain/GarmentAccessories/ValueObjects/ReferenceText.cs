namespace Reference.Domain.GarmentAccessories.ValueObjects;

/// <summary>Trims optional text, turns blank into null and rejects values longer than the column.</summary>
internal static class ReferenceText
{
    public static string? Optional(string? value, int maxLength, Func<int, Exception> tooLong)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : throw tooLong(maxLength);
    }
}
