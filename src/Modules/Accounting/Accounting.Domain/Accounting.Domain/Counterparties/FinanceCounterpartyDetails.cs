using System.Text.RegularExpressions;
using Accounting.Domain.Categories;
using Accounting.Domain.Errors;
using Accounting.Domain.Ledger;

namespace Accounting.Domain.Counterparties;

/// <summary>
/// Editable fields of a finance counterparty, already cleaned by <see cref="Of"/>. <see cref="Aliases"/> are other
/// spellings (one per line, for example the bank's), <see cref="DefaultCategoryId"/> is a category of the current books
/// suggested for its operations.
/// </summary>
public sealed partial record FinanceCounterpartyDetails(
    string Name,
    string Edrpou,
    string Iban,
    string BankName,
    string Aliases,
    FinanceCategoryId? DefaultCategoryId,
    bool AutoApplyCategory,
    string Note)
{
    public const int NameMaxLength = 250;
    public const int EdrpouMaxLength = 32;
    public const int IbanMaxLength = 64;
    public const int AliasMaxLength = 250;
    public const int MaxAliases = 20;
    public const int NoteMaxLength = 600;

    public IReadOnlyList<string> AliasList => Aliases.Length == 0 ? [] : Aliases.Split('\n');

    /// <summary>
    /// Trims the texts, keeps only digits of ЄДРПОУ/ІПН, only letters and digits of the IBAN (upper-case), and makes
    /// the alternative names one per line without repeats.
    /// </summary>
    public static DomainResult<FinanceCounterpartyDetails> Of(string? name, string? edrpou, string? iban, string? bankName,
        IEnumerable<string>? aliases, FinanceCategoryId? defaultCategoryId, bool autoApplyCategory, string? note)
    {
        var cleanName = FinanceText.Name(name);
        if (cleanName.Length == 0)
            return AccountingErrors.Rule("Accounting.Counterparty.Name", "Вкажіть назву контрагента.");
        if (cleanName.Length > NameMaxLength)
            return AccountingErrors.Rule("Accounting.Counterparty.Name", $"Назва — не більше {NameMaxLength} символів.");

        var cleanEdrpou = Whitespace().Replace(edrpou ?? string.Empty, string.Empty);
        if (cleanEdrpou.Length > EdrpouMaxLength || cleanEdrpou.Any(c => !char.IsAsciiDigit(c)))
            return AccountingErrors.Rule("Accounting.Counterparty.Edrpou",
                $"ЄДРПОУ або ІПН — лише цифри, не більше {EdrpouMaxLength}.");

        var cleanIban = NotLetterOrDigit().Replace(iban ?? string.Empty, string.Empty).ToUpperInvariant();
        if (cleanIban.Length > IbanMaxLength)
            return AccountingErrors.Rule("Accounting.Counterparty.Iban", $"IBAN — не більше {IbanMaxLength} символів.");

        var cleanBankName = FinanceText.Name(bankName);
        if (cleanBankName.Length > NameMaxLength)
            return AccountingErrors.Rule("Accounting.Counterparty.BankName",
                $"Назва в банку — не більше {NameMaxLength} символів.");

        var cleanAliases = (aliases ?? []).Select(FinanceText.Name).Where(x => x.Length > 0)
            .Where(x => !string.Equals(x, cleanName, StringComparison.OrdinalIgnoreCase))
            .DistinctBy(x => x.ToLowerInvariant()).ToList();
        if (cleanAliases.Count > MaxAliases || cleanAliases.Any(x => x.Length > AliasMaxLength))
            return AccountingErrors.Rule("Accounting.Counterparty.Aliases",
                $"Інших назв — не більше {MaxAliases}, кожна до {AliasMaxLength} символів.");

        var cleanNote = FinanceText.Clean(note);
        if (cleanNote.Length > NoteMaxLength)
            return AccountingErrors.Rule("Accounting.Counterparty.Note", $"Примітка — не більше {NoteMaxLength} символів.");

        return new FinanceCounterpartyDetails(cleanName, cleanEdrpou, cleanIban, cleanBankName,
            string.Join('\n', cleanAliases), defaultCategoryId, autoApplyCategory, cleanNote);
    }

    /// <summary>Lower-case text that the list search looks in: names, codes and IBAN.</summary>
    public string SearchText() =>
        string.Join('\n', new[] { Name, BankName, Edrpou, Iban }.Concat(AliasList).Where(x => x.Length > 0))
            .ToLowerInvariant();

    [GeneratedRegex(@"\s")]
    private static partial Regex Whitespace();

    [GeneratedRegex(@"[^\p{L}\p{N}]")]
    private static partial Regex NotLetterOrDigit();
}
