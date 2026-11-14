using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Domain.Transfers;
using Accounting.Domain.Statements.Enums;
using Accounting.Domain.Statements.ValueObjects;
using BuildingBlocks.Domain.Entity;

namespace Accounting.Domain.Statements;

/// <summary>
/// A row of an uploaded statement with what the import recognised and what the director chose for it. The bank data
/// never change; the classification changes only while the statement is a preview. After booking the line points to
/// the operation or transfer it became (for the rollback of stage 7).
/// </summary>
public sealed class BankStatementLine : IEntity<BankStatementLineId>
{
    private BankStatementLine() { }

    public BankStatementLineId Id { get; private set; }
    public BankStatementId StatementId { get; private set; }
    public int RowNumber { get; private set; }
    public DateOnly? Date { get; private set; }
    public TimeOnly? Time { get; private set; }
    public CategoryDirection? Type { get; private set; }
    public long? AmountMinor { get; private set; }
    public string? Currency { get; private set; }
    public string BankName { get; private set; } = null!;
    public string Edrpou { get; private set; } = null!;
    public string Iban { get; private set; } = null!;
    public string Purpose { get; private set; } = null!;
    public string Document { get; private set; } = null!;
    public string? BankId { get; private set; }
    public string? Fingerprint { get; private set; }
    public string Error { get; private set; } = null!;
    public string RawJson { get; private set; } = null!;

    public BankDuplicate Duplicate { get; private set; }

    /// <summary>Why the line looks like a duplicate: the record it repeats, as shown to the director.</summary>
    public string DuplicateNote { get; private set; } = null!;

    /// <summary>The other own account when the counterparty IBAN is one of ours: the line is a transfer.</summary>
    public FinanceAccountId? TransferAccountId { get; private set; }

    public CounterpartyKind? CounterpartyKind { get; private set; }
    public string? CounterpartyId { get; private set; }

    /// <summary>The name of the chosen counterparty when it was chosen, for the list.</summary>
    public string CounterpartyName { get; private set; } = null!;

    /// <summary>False for an unconfirmed finance counterparty; such a line still needs a look.</summary>
    public bool CounterpartyVerified { get; private set; }

    public FinanceCategoryId? CategoryId { get; private set; }
    public string OneoffName { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Comment { get; private set; } = null!;
    public bool Excluded { get; private set; }
    public BankLineState State { get; private set; }

    public FinanceTransactionId? TransactionId { get; private set; }
    public FinanceTransferId? TransferId { get; private set; }

    /// <summary>«Провести виписку» created the finance counterparty of this line («Відкотити» may archive it).</summary>
    public bool CreatedCounterparty { get; private set; }

    public CounterpartyLink? Counterparty =>
        CounterpartyKind is { } kind && CounterpartyId is { } id ? new CounterpartyLink(kind, id) : null;

    public bool HasParty => BankName.Length > 0 || Edrpou.Length > 0 || Iban.Length > 0;

    /// <summary>«Провести виписку» creates a finance counterparty for a recognised-by-nobody party of an operation.</summary>
    public bool NeedsNewCounterparty =>
        TransferAccountId is null && Counterparty is null && OneoffName.Length == 0 && HasParty;

    /// <summary>One new counterparty per ЄДРПОУ, else IBAN, else bank name (case ignored).</summary>
    public string PartyKey => Edrpou.Length > 0 ? "code:" + Edrpou
        : Iban.Length > 0 ? "iban:" + Iban
        : "name:" + BankName.ToLowerInvariant();

    /// <summary>
    /// The unconfirmed finance counterparty «Провести виписку» creates for this line's party: named after the bank's
    /// name, else the ЄДРПОУ, else the IBAN; codes too long for the card are left out.
    /// </summary>
    public DomainResult<FinanceCounterparty> NewCounterparty(FinanceCounterpartyId id, string statementFileName,
        DateTimeOffset now)
    {
        var name = BankName.Length > 0 ? BankName
            : Edrpou.Length > 0 ? $"ЄДРПОУ {Edrpou}"
            : $"IBAN {Iban}";
        var details = FinanceCounterpartyDetails.Of(name,
            Edrpou.Length <= FinanceCounterpartyDetails.EdrpouMaxLength ? Edrpou : null,
            Iban.Length <= FinanceCounterpartyDetails.IbanMaxLength ? Iban : null, BankName, null,
            null, false, $"Створено з виписки «{statementFileName}».");
        if (details.IsFailure) return details.Error!;
        return FinanceCounterparty.Create(id, details.Value, null, verified: false, now);
    }

    /// <summary>Whether «Провести виписку» turns this line into a record.</summary>
    public bool WillBook => !Excluded && State != BankLineState.Error && Duplicate != BankDuplicate.Exact;

    public static BankStatementLine Create(BankStatementId statementId, BankLineData data)
    {
        var line = new BankStatementLine
        {
            Id = BankStatementLineId.New(),
            StatementId = statementId,
            RowNumber = data.RowNumber,
            Date = data.Date,
            Time = data.Time,
            Type = data.Type,
            AmountMinor = data.AmountMinor,
            Currency = data.Currency,
            BankName = Cut(FinanceText.Name(data.BankName), BankLineData.NameMaxLength),
            Edrpou = Cut(data.Edrpou.Trim(), BankLineData.CodeMaxLength),
            Iban = Cut(data.Iban.Trim(), BankLineData.CodeMaxLength),
            Purpose = Cut(FinanceText.Clean(data.Purpose), BankLineData.PurposeMaxLength),
            Document = Cut(data.Document.Trim(), BankLineData.DocumentMaxLength),
            BankId = data.BankId is null ? null : Cut(data.BankId.Trim(), BankLineData.BankIdMaxLength),
            Fingerprint = data.Fingerprint,
            Error = Cut(data.Error, BankLineData.ErrorMaxLength),
            RawJson = Cut(data.RawJson, BankLineData.RawJsonMaxLength),
            DuplicateNote = string.Empty,
            CounterpartyName = string.Empty,
            OneoffName = string.Empty,
            Description = string.Empty,
            Comment = string.Empty,
        };
        if (line.Error.Length == 0 && (line.Date is null || line.Type is null || line.AmountMinor is null))
            line.Error = "Рядок не прочитано.";
        line.Refresh();
        return line;
    }

    /// <summary>
    /// What the import recognised: a duplicate with its reason, another own account of a transfer, the counterparty and
    /// the category it suggests. A duplicate starts excluded.
    /// </summary>
    public void Recognise(BankDuplicate duplicate, string duplicateNote, FinanceAccountId? transferAccountId,
        CounterpartyState? counterparty, FinanceCategoryId? categoryId)
    {
        Duplicate = Error.Length > 0 ? BankDuplicate.None : duplicate;
        DuplicateNote = Duplicate == BankDuplicate.None ? string.Empty : Cut(duplicateNote, BankLineData.ErrorMaxLength);
        TransferAccountId = Error.Length > 0 ? null : transferAccountId;
        SetCounterparty(TransferAccountId is null ? counterparty : null);
        CategoryId = TransferAccountId is null ? categoryId : null;
        Excluded = Duplicate != BankDuplicate.None;
        Refresh();
    }

    /// <summary>
    /// The director's choice for the line while the statement is a preview. <paramref name="category"/> must be of the
    /// current books and the direction of the line; a transfer line takes only the note and the include switch.
    /// </summary>
    public DomainResult Edit(FinanceCategory? category, CounterpartyState? counterparty, string? oneoffName, string? description,
        string? comment, bool excluded)
    {
        if (!excluded && Error.Length > 0)
            return AccountingErrors.Rule("Accounting.Statement.Line", "Рядок з помилкою не можна провести.");
        if (!excluded && Duplicate == BankDuplicate.Exact)
            return AccountingErrors.Rule("Accounting.Statement.Line", "Цей рядок уже є в обліку; його не проводять удруге.");

        var checkedComment = MovementRules.Text(comment, FinanceTransactionDetails.CommentMaxLength, "Примітка");
        if (checkedComment.IsFailure) return checkedComment.WithoutValue();
        var cleanComment = checkedComment.Value;
        if (TransferAccountId is not null)
        {
            if (category is not null || counterparty is not null || !string.IsNullOrWhiteSpace(oneoffName))
                return AccountingErrors.Rule("Accounting.Statement.Line",
                    "Переказ між своїми рахунками не має категорії і контрагента.");
            Comment = cleanComment;
            Excluded = excluded;
            Refresh();
            return DomainResult.Success();
        }

        if (category is not null)
        {
            if (category.Scope != LedgerScope.Current)
                return AccountingErrors.Rule("Accounting.Statement.Category", "Оберіть категорію поточного обліку.");
            if (category.Direction != Type)
                return AccountingErrors.Rule("Accounting.Statement.Category", Type == CategoryDirection.Income
                    ? "Для доходу оберіть категорію доходів."
                    : "Для витрати оберіть категорію витрат.");
            if (!category.Active && category.Id != CategoryId)
                return AccountingErrors.Rule("Accounting.Statement.Category", $"Категорія «{category.Name}» вимкнена.");
        }

        if (counterparty is { Active: false } && counterparty.Link != Counterparty)
            return AccountingErrors.Rule("Accounting.Statement.Counterparty",
                $"Контрагент «{counterparty.Name}» в архіві. Оберіть іншого.");
        var oneoff = FinanceTransactionDetails.OneoffOf(counterparty?.Link, oneoffName);
        if (oneoff.IsFailure) return oneoff.WithoutValue();
        var cleanDescription = MovementRules.Text(description, FinanceTransactionDetails.DescriptionMaxLength, "Опис",
            oneLine: true);
        if (cleanDescription.IsFailure) return cleanDescription.WithoutValue();

        CategoryId = category?.Id;
        SetCounterparty(counterparty);
        OneoffName = oneoff.Value;
        Description = cleanDescription.Value;
        Comment = cleanComment;
        Excluded = excluded;
        Refresh();
        return DomainResult.Success();
    }

    /// <summary>The books got the same line meanwhile (checked again at booking): it is skipped.</summary>
    public void FoundInBooks(string note)
    {
        Duplicate = BankDuplicate.Exact;
        DuplicateNote = Cut(note, BankLineData.ErrorMaxLength);
        Excluded = true;
        Refresh();
    }

    /// <summary>A finance counterparty created for the line when the statement is booked.</summary>
    public void UseCreatedCounterparty(CounterpartyState counterparty)
    {
        SetCounterparty(counterparty);
        CreatedCounterparty = true;
    }

    /// <summary>
    /// «Відкотити виписку»: the line forgets its operation or transfer. A counterparty the booking created is refreshed
    /// to <paramref name="created"/>, its state now, or dropped (null) when the rollback archived it, so the line waits
    /// for a counterparty again.
    /// </summary>
    public void Unbook(CounterpartyState? created)
    {
        TransactionId = null;
        TransferId = null;
        if (CreatedCounterparty)
        {
            SetCounterparty(created);
            CreatedCounterparty = created is not null;
        }

        Refresh();
    }

    public void Booked(FinanceTransactionId transactionId) => TransactionId = transactionId;

    public void Booked(FinanceTransferId transferId) => TransferId = transferId;

    private void SetCounterparty(CounterpartyState? counterparty)
    {
        CounterpartyKind = counterparty?.Link.Kind;
        CounterpartyId = counterparty?.Link.Id;
        CounterpartyName = counterparty?.Name ?? string.Empty;
        CounterpartyVerified = counterparty?.Verified ?? false;
        if (counterparty is not null) OneoffName = string.Empty;
    }

    private void Refresh() =>
        State = Error.Length > 0 ? BankLineState.Error
            : Duplicate != BankDuplicate.None ? BankLineState.Duplicate
            : TransferAccountId is not null ? BankLineState.Transfer
            : Counterparty is null && OneoffName.Length == 0 && HasParty || Counterparty is not null && !CounterpartyVerified
                ? BankLineState.NewCounterparty
            : CategoryId is null ? BankLineState.NeedsCategory
            : BankLineState.Ready;

    private static string Cut(string value, int max) => value.Length <= max ? value : value[..max];
}
