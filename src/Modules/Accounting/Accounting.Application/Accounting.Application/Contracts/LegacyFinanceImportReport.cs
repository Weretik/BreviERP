using System.Text.Json.Serialization;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;

namespace Accounting.Application.Contracts;

/// <summary>
/// What the move of the old Calculator «Фінанси» finds (a check) or did (<c>Applied</c>). <c>CanApply</c> is false while
/// <c>Errors</c> is not empty; <c>ErrorCount</c> counts every error, <c>Errors</c> lists the first 100. Totals and
/// balances are counted from the file, so they reconcile with the old system.
/// </summary>
public sealed record LegacyFinanceImportReport(
    bool Applied,
    bool CanApply,
    DateTimeOffset? ExportedAt,
    IReadOnlyList<LegacyImportCountResponse> Counts,
    LegacyImportPartiesResponse Parties,
    IReadOnlyList<LegacyImportTotalResponse> Totals,
    IReadOnlyList<LegacyImportBalanceResponse> Balances,
    int ErrorCount,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings);

/// <summary>Records of one kind: in the file, to add, already moved earlier (same id), left out on purpose.</summary>
public sealed record LegacyImportCountResponse(LegacyImportKind Kind, int InFile, int ToAdd, int AlreadyMoved, int Skipped);

[JsonConverter(typeof(JsonStringEnumConverter<LegacyImportKind>))]
public enum LegacyImportKind
{
    Accounts,
    Categories,
    Counterparties,
    Transactions,
    Transfers,
    AuditEntries,
}

/// <summary>
/// How the old clients and suppliers used in operations are linked: to «Клієнти»/«Постачальники» of Brevi, or kept as
/// finance counterparties when Brevi does not know them. <c>Unused</c> ones (no operation) are not moved.
/// </summary>
public sealed record LegacyImportPartiesResponse(int ClientsLinked, int ClientsAsFinancial, int SuppliersLinked,
    int SuppliersAsFinancial, int Unused);

/// <summary>Movements of the file by area, kind and state, in kopecks.</summary>
public sealed record LegacyImportTotalResponse(LedgerScope Scope, LegacyImportMovement Movement, MovementStatus Status,
    string Currency, int Count, long AmountMinor);

[JsonConverter(typeof(JsonStringEnumConverter<LegacyImportMovement>))]
public enum LegacyImportMovement
{
    Income,
    Expense,
    Transfer,
}

/// <summary>
/// An account after the move, counted from the file: <c>ArchiveMinor</c> — balance on 30.09.2026 (opening plus active
/// archive movements); <c>CurrentMinor</c> — today (confirmed balance on 01.10 plus the current books), null while that
/// balance is not confirmed; <c>DifferenceMinor</c> — confirmed balance minus the archive one when they differ.
/// </summary>
public sealed record LegacyImportBalanceResponse(Guid AccountId, string Name, string Currency, bool Active,
    long ArchiveMinor, long? CutoverMinor, long? CurrentMinor, long? DifferenceMinor);
