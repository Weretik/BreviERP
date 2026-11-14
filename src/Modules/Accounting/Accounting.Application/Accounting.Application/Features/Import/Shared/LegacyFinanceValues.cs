using System.Globalization;
using System.Text.Json;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;

namespace Accounting.Application.Features.Import.Shared;

/// <summary>The values of the old export (upper-case codes, ISO dates) in the terms of this module.</summary>
internal static class LegacyFinanceValues
{
    public const string UnknownAuthor = "Калькулятор";
    private static readonly CultureInfo Money = CultureInfo.GetCultureInfo("uk-UA");

    public static Guid? Id(string? value) => Guid.TryParse(value, out var id) && id != Guid.Empty ? id : null;

    public static DateOnly? Date(string? value) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;

    public static DateTimeOffset? Moment(string? value) =>
        DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var at)
            ? at.ToUniversalTime()
            : null;

    public static CategoryDirection? Direction(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "INCOME" => CategoryDirection.Income,
        "EXPENSE" => CategoryDirection.Expense,
        _ => null,
    };

    public static LedgerScope? Scope(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "LEGACY_ARCHIVE" => LedgerScope.LegacyArchive,
        "CURRENT" => LedgerScope.Current,
        _ => null,
    };

    public static MovementSource? Source(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "HOME_ACCOUNTING" => MovementSource.HomeAccountingImport,
        "BANK_IMPORT" => MovementSource.BankImport,
        "MANUAL" => MovementSource.Manual,
        _ => null,
    };

    public static MovementStatus? Status(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "ACTIVE" => MovementStatus.Active,
        "VOID" => MovementStatus.Void,
        _ => null,
    };

    public static VerificationStatus? Verification(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "VERIFIED" => VerificationStatus.Verified,
        "NEEDS_CATEGORY" => VerificationStatus.NeedsCategory,
        "NEEDS_COUNTERPARTY_REVIEW" => VerificationStatus.NeedsCounterpartyReview,
        _ => null,
    };

    public static FinanceAccountType? AccountType(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "bank" => FinanceAccountType.Bank,
        "card" => FinanceAccountType.Card,
        "cash" => FinanceAccountType.Cash,
        "other" => FinanceAccountType.Other,
        _ => null,
    };

    /// <summary>An old log action in the terms of «Журнал змін»; null when it has no counterpart.</summary>
    public static AuditAction? Action(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "CREATE" or "BANK_IMPORT" or "CREATE_FINANCIAL" or "BANK_IMPORT_CREATE" => AuditAction.Create,
        "UPDATE" or "BANK_DETAILS_UPDATE" => AuditAction.Update,
        "VOID" or "ROLLBACK" or "BANK_IMPORT_ROLLBACK" => AuditAction.Void,
        "REPLAY" or "BANK_IMPORT_REPLAY" => AuditAction.Restore,
        "VERIFY" => AuditAction.Verify,
        "CUTOVER_BALANCE_CONFIRM" => AuditAction.CutoverConfirm,
        "CUTOVER_BALANCE_RESET" => AuditAction.CutoverReset,
        _ => null,
    };

    /// <summary>The author as text of the log: the old e-mail or system name, trimmed to the column.</summary>
    public static string Author(string? value)
    {
        var clean = (value ?? string.Empty).Trim();
        if (clean.Length == 0) return UnknownAuthor;
        return clean.Length <= MovementRules.ByMaxLength ? clean : clean[..MovementRules.ByMaxLength];
    }

    /// <summary>A JSON snapshot as is, or the text wrapped as a JSON string when it is not valid JSON.</summary>
    public static string? Snapshot(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try
        {
            using var _ = JsonDocument.Parse(value);
            return value;
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(value);
        }
    }

    public static string Amount(long minor) => (minor / 100m).ToString("N2", Money);
}
