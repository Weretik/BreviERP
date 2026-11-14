using Accounting.Domain;
using Accounting.Domain.Ledger;

namespace Accounting.Domain.Accounts;

/// <summary>
/// A place where Brevi keeps money: bank account, card, cash box. It has two separate starting points: the archive
/// opening balance (<see cref="OpeningMinor"/> on <see cref="OpeningDate"/>) and the balance on
/// <see cref="LedgerCalendar.CutoverDate"/> that the director confirms by hand for the current books. An unconfirmed
/// cutover balance is unknown, not zero.
/// </summary>
public sealed class FinanceAccount : IAggregateRoot
{
    public const int ByMaxLength = 256;

    private FinanceAccount() { }

    private FinanceAccount(FinanceAccountId id, FinanceAccountDetails details, DateTimeOffset now)
    {
        Id = id;
        Apply(details);
        Active = true;
        Version = 1;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public FinanceAccountId Id { get; private set; }
    public string Name { get; private set; } = null!;
    public FinanceAccountType Type { get; private set; }

    /// <summary>Three upper-case letters, for example UAH. Every movement of the account uses it.</summary>
    public string Currency { get; private set; } = null!;

    /// <summary>Letters and digits only, upper-case; empty when not set. No bank checksum check.</summary>
    public string Iban { get; private set; } = null!;

    /// <summary>Archive opening balance in kopecks; zero and negative values are allowed.</summary>
    public long OpeningMinor { get; private set; }

    public DateOnly OpeningDate { get; private set; }

    /// <summary>Starting balance of the current books in kopecks; null while not set.</summary>
    public long? CutoverBalanceMinor { get; private set; }

    public bool CutoverConfirmed { get; private set; }
    public DateTimeOffset? CutoverConfirmedAt { get; private set; }

    /// <summary>E-mail of the staff account that confirmed the cutover balance.</summary>
    public string? CutoverConfirmedBy { get; private set; }

    public string Note { get; private set; } = null!;

    /// <summary>False after «Архівувати»: the account and its history stay, new current movements cannot use it.</summary>
    public bool Active { get; private set; }

    public int SortOrder { get; private set; }

    /// <summary>Optimistic version: starts at 1 and grows with every saved change.</summary>
    public int Version { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static FinanceAccount Create(FinanceAccountId id, FinanceAccountDetails details, DateTimeOffset now) =>
        new(id, details, now);

    /// <summary>An account of the old Calculator «Фінанси» with its id, balance on 01.10 and archive state (stage 5).</summary>
    public static DomainResult<FinanceAccount> FromLegacy(FinanceAccountId id, FinanceAccountDetails details, long? cutoverMinor,
        bool cutoverConfirmed, DateTimeOffset? cutoverConfirmedAt, string? cutoverConfirmedBy, bool active,
        DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        if (cutoverConfirmed && cutoverMinor is null)
            return AccountingErrors.Rule("Accounting.Account.Cutover", "Підтверджений залишок на 01.10 без суми.");
        if (cutoverMinor is { } amount && !MoneyMinor.InRange(amount))
            return AccountingErrors.Rule("Accounting.Account.Cutover", "Залишок завеликий.");
        return new FinanceAccount(id, details, createdAt)
        {
            CutoverBalanceMinor = cutoverMinor,
            CutoverConfirmed = cutoverConfirmed,
            CutoverConfirmedAt = cutoverConfirmed ? cutoverConfirmedAt ?? updatedAt : null,
            CutoverConfirmedBy = cutoverConfirmed ? Shorten(cutoverConfirmedBy) : null,
            Active = active,
            UpdatedAt = updatedAt,
        };
    }

    private static string Shorten(string? by) =>
        string.IsNullOrWhiteSpace(by) ? "Калькулятор" : by.Trim()[..Math.Min(by.Trim().Length, ByMaxLength)];

    public FinanceAccountDetails Details =>
        new(Name, Type, Currency, Iban, OpeningMinor, OpeningDate, Note, SortOrder);

    /// <summary>
    /// Changes the editable fields; returns false when nothing differs (the version stays). Once the account has
    /// movements its currency is fixed and its opening date cannot move past the earliest one.
    /// </summary>
    public DomainResult<bool> Update(FinanceAccountDetails details, FinanceAccountUsage usage, DateTimeOffset now)
    {
        if (details == Details) return false;
        if (usage.HasHistory && details.Currency != Currency)
            return AccountingErrors.Rule("Accounting.Account.Currency",
                "По рахунку вже є рух коштів, його валюту змінити не можна.");
        if (usage.EarliestDate is { } earliest && details.OpeningDate > earliest)
            return AccountingErrors.Rule("Accounting.Account.OpeningDate",
                $"Найраніший запис рахунку — {earliest:dd.MM.yyyy}; дата початкового залишку не може бути пізнішою.");
        Apply(details);
        Touch(now);
        return true;
    }

    /// <summary>
    /// «Залишок на 01.10»: <paramref name="confirmed"/> stores the amount with who and when; otherwise the amount and
    /// the confirmation are cleared. Returns false when nothing changes.
    /// </summary>
    public DomainResult<bool> SetCutoverBalance(long? balanceMinor, bool confirmed, string by, DateTimeOffset now)
    {
        if (confirmed)
        {
            if (balanceMinor is not { } amount)
                return AccountingErrors.Rule("Accounting.Account.Cutover", "Вкажіть залишок на 01.10.2026.");
            if (!MoneyMinor.InRange(amount))
                return AccountingErrors.Rule("Accounting.Account.Cutover", "Залишок завеликий.");
            if (CutoverConfirmed && CutoverBalanceMinor == amount) return false;
            CutoverBalanceMinor = amount;
            CutoverConfirmed = true;
            CutoverConfirmedAt = now;
            CutoverConfirmedBy = by;
        }
        else
        {
            if (!CutoverConfirmed && CutoverBalanceMinor is null) return false;
            CutoverBalanceMinor = null;
            CutoverConfirmed = false;
            CutoverConfirmedAt = null;
            CutoverConfirmedBy = null;
        }

        Touch(now);
        return true;
    }

    /// <summary>«Архівувати» (false) or «Відновити» (true); returns false when the account is already in that state.</summary>
    public bool SetActive(bool active, DateTimeOffset now)
    {
        if (Active == active) return false;
        Active = active;
        Touch(now);
        return true;
    }

    private void Apply(FinanceAccountDetails details)
    {
        Name = details.Name;
        Type = details.Type;
        Currency = details.Currency;
        Iban = details.Iban;
        OpeningMinor = details.OpeningMinor;
        OpeningDate = details.OpeningDate;
        Note = details.Note;
        SortOrder = details.SortOrder;
    }

    private void Touch(DateTimeOffset now)
    {
        Version++;
        UpdatedAt = now;
    }
}
