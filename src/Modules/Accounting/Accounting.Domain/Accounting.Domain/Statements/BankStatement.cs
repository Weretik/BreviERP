using Accounting.Domain.Accounts;
using Accounting.Domain;
using Accounting.Domain.Movements;

namespace Accounting.Domain.Statements;

/// <summary>
/// An uploaded bank statement of one account («Виписки банку»). Its lines are kept apart (<see cref="BankStatementLine"/>)
/// and corrected while it is a <see cref="BankStatementStatus.Preview"/>; «Провести виписку» books them once and makes it
/// <see cref="BankStatementStatus.Booked"/>. The same file of the same account (<see cref="FileHash"/>) opens it again.
/// </summary>
public sealed class BankStatement : IAggregateRoot
{
    public const int FileNameMaxLength = 255;
    public const int MaxLines = 10_000;
    public const long MaxBytes = 12 * 1024 * 1024;

    private BankStatement() { }

    public BankStatementId Id { get; private set; }
    public FinanceAccountId AccountId { get; private set; }
    public string FileName { get; private set; } = null!;

    /// <summary>SHA-256 of the file, lowercase hex.</summary>
    public string FileHash { get; private set; } = null!;

    public BankStatementStatus Status { get; private set; }
    public int LineCount { get; private set; }
    public DateOnly? FirstDate { get; private set; }
    public DateOnly? LastDate { get; private set; }
    public int TransactionCount { get; private set; }
    public int TransferCount { get; private set; }
    public string CreatedBy { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public string? BookedBy { get; private set; }
    public DateTimeOffset? BookedAt { get; private set; }
    public int Version { get; private set; }

    /// <summary>A new preview of <paramref name="account"/> with its lines; the account must be active.</summary>
    public static DomainResult<(BankStatement Statement, IReadOnlyList<BankStatementLine> Lines)> Create(
        BankStatementId id,
        FinanceAccount account, string fileName, string fileHash, IReadOnlyList<BankLineData> lines, string by,
        DateTimeOffset now)
    {
        if (!account.Active)
            return AccountingErrors.Rule("Accounting.Statement.Account", $"Рахунок «{account.Name}» в архіві.");
        if (lines.Count == 0)
            return AccountingErrors.Rule("Accounting.Statement.Empty", "У виписці немає жодного рядка з операцією.");
        if (lines.Count > MaxLines)
            return AccountingErrors.Rule("Accounting.Statement.Size", $"У виписці більше {MaxLines} рядків.");
        if (fileHash.Length != 64 || string.IsNullOrWhiteSpace(by) || by.Length > MovementRules.ByMaxLength)
            throw new ArgumentException("File hash and author are required.");

        var name = fileName.Trim();
        if (name.Length == 0) name = "виписка";
        var statement = new BankStatement
        {
            Id = id,
            AccountId = account.Id,
            FileName = name.Length <= FileNameMaxLength ? name : name[..FileNameMaxLength],
            FileHash = fileHash,
            Status = BankStatementStatus.Preview,
            LineCount = lines.Count,
            FirstDate = lines.Min(x => x.Date),
            LastDate = lines.Max(x => x.Date),
            CreatedBy = by,
            CreatedAt = now,
            Version = 1,
        };
        IReadOnlyList<BankStatementLine> statementLines = lines.Select(x => BankStatementLine.Create(id, x)).ToList();
        return (statement, statementLines);
    }

    /// <summary>Lines change only while the statement is not booked.</summary>
    public DomainResult CheckPreview() => Status == BankStatementStatus.Preview
        ? DomainResult.Success()
        : AccountingErrors.Rule("Accounting.Statement.Booked", "Виписку вже проведено; рядки не змінюються.");

    /// <summary>«Провести виписку»: remembers how many operations and transfers it added.</summary>
    public DomainResult Book(int transactions, int transfers, string by, DateTimeOffset now)
    {
        if (CheckPreview() is { IsFailure: true } booked) return booked;
        if (transactions + transfers == 0)
            return AccountingErrors.Rule("Accounting.Statement.Nothing", "Немає жодного рядка, який можна провести.");
        Status = BankStatementStatus.Booked;
        TransactionCount = transactions;
        TransferCount = transfers;
        BookedBy = by;
        BookedAt = now;
        Version++;
        return DomainResult.Success();
    }

    /// <summary>«Відкотити виписку»: the statement is a preview again; its operations and transfers were cancelled.</summary>
    public DomainResult Rollback()
    {
        if (Status != BankStatementStatus.Booked)
            return AccountingErrors.Rule("Accounting.Statement.NotBooked", "Виписку ще не проведено; відкочувати нічого.");
        Status = BankStatementStatus.Preview;
        TransactionCount = 0;
        TransferCount = 0;
        BookedBy = null;
        BookedAt = null;
        Version++;
        return DomainResult.Success();
    }
}
