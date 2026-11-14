using Accounting.Domain.Accounts;
using Accounting.Domain;
using Accounting.Domain.Ledger;

namespace Accounting.Domain.Movements;

/// <summary>Date, area and account rules shared by operations and transfers.</summary>
public static class MovementRules
{
    public const int ByMaxLength = 256;

    /// <summary>
    /// A new date must fall into the area of the record (before 01.10.2026 — «Домашня бухгалтерія», from it on — the
    /// current books). An unchanged date of an existing record is kept as is.
    /// </summary>
    public static DomainResult CheckDate(DateOnly date, LedgerScope scope, DateOnly? previous)
    {
        if (date < FinanceAccountDetails.EarliestDate || date > FinanceAccountDetails.LatestDate)
            return AccountingErrors.Rule("Accounting.Movement.Date", "Дата поза допустимими межами.");
        if (date == previous) return DomainResult.Success();
        if (LedgerCalendar.ScopeOf(date) != scope)
            return AccountingErrors.Rule("Accounting.Movement.Date", scope == LedgerScope.Current
                ? "Дата поточного обліку — з 01.10.2026. Для давніших записів відкрийте «Домашня бухгалтерія»."
                : "Дата «Домашньої бухгалтерії» — до 30.09.2026. Новіші записи додавайте в поточний облік.");
        return DomainResult.Success();
    }

    /// <summary>
    /// The account must exist from the date on; the current books need an active account, the archive accepts an
    /// archived one. An account that the record already had stays allowed after its archiving.
    /// </summary>
    public static DomainResult CheckAccount(FinanceAccount account, DateOnly date, LedgerScope scope, bool unchanged)
    {
        if (date < account.OpeningDate)
            return AccountingErrors.Rule("Accounting.Movement.Account",
                $"Рахунок «{account.Name}» ведеться з {account.OpeningDate:dd.MM.yyyy}; дата раніша.");
        if (scope == LedgerScope.Current && !account.Active && !unchanged)
            return AccountingErrors.Rule("Accounting.Movement.Account", $"Рахунок «{account.Name}» в архіві.");
        return DomainResult.Success();
    }

    public static DomainResult CheckAmount(long amountMinor)
    {
        if (amountMinor <= 0)
            return AccountingErrors.Rule("Accounting.Movement.Amount", "Сума має бути більшою за нуль.");
        if (amountMinor > MoneyMinor.Max)
            return AccountingErrors.Rule("Accounting.Movement.Amount", "Сума завелика.");
        return DomainResult.Success();
    }

    public static DomainResult CheckScope(LedgerScope scope)
    {
        if (!Enum.IsDefined(scope))
            return AccountingErrors.Rule("Accounting.Movement.Scope", "Невідома область обліку.");
        return DomainResult.Success();
    }

    /// <summary>Trimmed text no longer than <paramref name="max"/>; <paramref name="label"/> names the field in the message.</summary>
    public static DomainResult<string> Text(string? value, int max, string label, bool oneLine = false)
    {
        var clean = oneLine ? FinanceText.Name(value) : FinanceText.Clean(value);
        if (clean.Length > max)
            return AccountingErrors.Rule("Accounting.Movement.Text", $"{label} — не більше {max} символів.");
        return clean;
    }

    public static DomainResult EnsureActive(MovementStatus status)
    {
        if (status != MovementStatus.Active)
            return AccountingErrors.Rule("Accounting.Movement.Void", "Запис анульовано, його не можна змінити.");
        return DomainResult.Success();
    }
}
