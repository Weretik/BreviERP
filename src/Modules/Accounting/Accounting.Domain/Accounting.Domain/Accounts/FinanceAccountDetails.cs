using System.Text.RegularExpressions;
using Accounting.Domain.Errors;
using Accounting.Domain.Ledger;

namespace Accounting.Domain.Accounts;

/// <summary>Editable fields of an account («Рахунки і каси»), already cleaned by <see cref="Of"/>.</summary>
public sealed partial record FinanceAccountDetails(
    string Name,
    FinanceAccountType Type,
    string Currency,
    string Iban,
    long OpeningMinor,
    DateOnly OpeningDate,
    string Note,
    int SortOrder)
{
    public const int NameMaxLength = 120;
    public const int IbanMaxLength = 64;
    public const int NoteMaxLength = 600;
    public static readonly DateOnly EarliestDate = new(2000, 1, 1);
    public static readonly DateOnly LatestDate = new(2100, 12, 31);

    /// <summary>
    /// Trims the texts, upper-cases the currency, keeps only letters and digits of the IBAN (upper-case) and checks the
    /// rules. Throws <see cref="DomainException"/> with a message for the user.
    /// </summary>
    public static FinanceAccountDetails Of(string? name, FinanceAccountType type, string? currency, string? iban,
        long openingMinor, DateOnly openingDate, string? note, int sortOrder)
    {
        var cleanName = FinanceText.Name(name);
        if (cleanName.Length == 0) throw AccountingDomainError.Fail("Accounting.Account.Name", "Вкажіть назву рахунку.");
        if (cleanName.Length > NameMaxLength)
            throw AccountingDomainError.Fail("Accounting.Account.Name", $"Назва рахунку — не більше {NameMaxLength} символів.");
        if (!Enum.IsDefined(type)) throw AccountingDomainError.Fail("Accounting.Account.Type", "Невідомий тип рахунку.");

        var cleanCurrency = FinanceText.Clean(currency).ToUpperInvariant();
        if (!CurrencyPattern().IsMatch(cleanCurrency))
            throw AccountingDomainError.Fail("Accounting.Account.Currency", "Валюта — три латинські літери, наприклад UAH.");

        var cleanIban = NotLetterOrDigit().Replace(iban ?? string.Empty, string.Empty).ToUpperInvariant();
        if (cleanIban.Length > IbanMaxLength)
            throw AccountingDomainError.Fail("Accounting.Account.Iban", $"IBAN або номер — не більше {IbanMaxLength} символів.");
        if (!MoneyMinor.InRange(openingMinor))
            throw AccountingDomainError.Fail("Accounting.Account.Opening", "Початковий залишок завеликий.");
        if (openingDate < EarliestDate || openingDate > LatestDate)
            throw AccountingDomainError.Fail("Accounting.Account.OpeningDate", "Дата початкового залишку поза допустимими межами.");

        var cleanNote = FinanceText.Clean(note);
        if (cleanNote.Length > NoteMaxLength)
            throw AccountingDomainError.Fail("Accounting.Account.Note", $"Примітка — не більше {NoteMaxLength} символів.");

        return new FinanceAccountDetails(cleanName, type, cleanCurrency, cleanIban, openingMinor, openingDate, cleanNote,
            sortOrder);
    }

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex CurrencyPattern();

    [GeneratedRegex(@"[^\p{L}\p{N}]")]
    private static partial Regex NotLetterOrDigit();
}
