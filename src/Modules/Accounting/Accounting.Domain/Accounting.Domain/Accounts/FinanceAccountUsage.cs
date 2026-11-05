namespace Accounting.Domain.Accounts;

/// <summary>
/// Whether an account has any operation or transfer (cancelled ones too) and the date of the earliest one. An account
/// with history can only be archived, not deleted.
/// </summary>
public sealed record FinanceAccountUsage(bool HasHistory, DateOnly? EarliestDate)
{
    public static readonly FinanceAccountUsage None = new(false, null);
}
