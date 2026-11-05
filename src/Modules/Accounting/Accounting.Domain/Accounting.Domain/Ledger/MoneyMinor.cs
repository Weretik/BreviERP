namespace Accounting.Domain.Ledger;

/// <summary>Money is a whole number of kopecks (minor units); at most 12 digits before the decimal point.</summary>
public static class MoneyMinor
{
    public const long Max = 99_999_999_999_999;

    public static bool InRange(long value) => value is >= -Max and <= Max;
}
