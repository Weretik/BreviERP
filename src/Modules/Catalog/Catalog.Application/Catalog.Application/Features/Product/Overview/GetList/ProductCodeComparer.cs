namespace Catalog.Application.Features.Product.Overview.GetList;

/// <summary>Orders "MOD-2" before "MOD-10" and PPE article "9" before "17".</summary>
public sealed class ProductCodeComparer : IComparer<string?>
{
    public static readonly ProductCodeComparer Instance = new();

    public int Compare(string? x, string? y)
    {
        var (xPrefix, xNumber) = Split(x);
        var (yPrefix, yNumber) = Split(y);
        var byPrefix = string.CompareOrdinal(xPrefix, yPrefix);
        if (byPrefix != 0) return byPrefix;
        var byNumber = xNumber.CompareTo(yNumber);
        return byNumber != 0 ? byNumber : string.CompareOrdinal(x, y);
    }

    private static (string Prefix, long Number) Split(string? value)
    {
        value ??= string.Empty;
        var end = value.Length;
        while (end > 0 && char.IsAsciiDigit(value[end - 1])) end--;
        return long.TryParse(value.AsSpan(end), out var number) && value.Length - end <= 18
            ? (value[..end], number)
            : (value, long.MaxValue);
    }
}
