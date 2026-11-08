using System.Text.Json;
using System.Text.Json.Serialization;

namespace Accounting.Application.Import;

/// <summary>
/// The «brevi-finance-v5» export of the old Calculator «Фінанси» (snake_case rows, 0/1 flags). Only what Brevi keeps is
/// read; bank statement previews and their links stay out of the move.
/// </summary>
internal sealed record LegacyFinanceFile(
    string? Format,
    [property: JsonPropertyName("exportedAt")] string? ExportedAt,
    bool Complete,
    IReadOnlyList<LegacyAccountRow>? Accounts,
    IReadOnlyList<LegacyCategoryRow>? Categories,
    IReadOnlyList<LegacyCounterpartyRow>? Counterparties,
    IReadOnlyList<LegacyTransactionRow>? Transactions,
    IReadOnlyList<LegacyTransferRow>? Transfers,
    IReadOnlyList<JsonElement>? Attachments,
    [property: JsonPropertyName("auditLog")] IReadOnlyList<LegacyAuditRow>? AuditLog,
    [property: JsonPropertyName("transferAuditLog")] IReadOnlyList<LegacyAuditRow>? TransferAuditLog,
    [property: JsonPropertyName("categoryAuditLog")] IReadOnlyList<LegacyAuditRow>? CategoryAuditLog,
    [property: JsonPropertyName("counterpartyAuditLog")] IReadOnlyList<LegacyAuditRow>? CounterpartyAuditLog,
    [property: JsonPropertyName("accountAuditLog")] IReadOnlyList<LegacyAuditRow>? AccountAuditLog,
    IReadOnlyList<LegacySettingRow>? Settings);

internal sealed record LegacyAccountRow(
    string Id,
    string? Name,
    string? Type,
    string? Currency,
    long OpeningMinor,
    string? OpeningDate,
    string? Iban,
    long? CutoverBalanceMinor,
    bool CutoverBalanceConfirmed,
    string? CutoverBalanceAt,
    string? CutoverBalanceBy,
    string? Note,
    bool Active,
    int SortOrder,
    string? CreatedAt,
    string? UpdatedAt);

internal sealed record LegacyCategoryRow(
    string Id,
    string? Name,
    string? Direction,
    string? ParentId,
    bool Active,
    string? LedgerScope,
    int SortOrder,
    string? Note,
    string? CreatedAt,
    string? UpdatedAt);

internal sealed record LegacyCounterpartyRow(
    string Id,
    string? Name,
    string? ExternalSource,
    string? ExternalType,
    string? Edrpou,
    string? Iban,
    string? BankName,
    string? AliasesJson,
    bool Verified,
    bool Active,
    string? KeepinId,
    string? SupplierId,
    string? CreatedAt,
    string? UpdatedAt);

internal sealed record LegacyTransactionRow(
    string Id,
    string? TransactionDate,
    string? Type,
    string? AccountId,
    long AmountMinor,
    string? Currency,
    string? CategoryId,
    string? CounterpartyId,
    string? Description,
    string? Comment,
    string? Source,
    string? ExternalId,
    string? Fingerprint,
    string? VerificationStatus,
    string? BankTransactionId,
    string? BankPurpose,
    string? LedgerScope,
    string? CreatedBy,
    string? CreatedAt,
    string? UpdatedAt,
    string? Status,
    int Version,
    string? OneoffName);

internal sealed record LegacyTransferRow(
    string Id,
    string? TransferDate,
    string? FromAccountId,
    string? ToAccountId,
    long AmountMinor,
    string? Currency,
    string? Note,
    string? PersonalNote,
    string? Source,
    string? LedgerScope,
    string? ExternalId,
    string? Fingerprint,
    string? CreatedBy,
    string? CreatedAt,
    string? UpdatedAt,
    string? Status,
    int Version);

/// <summary>A row of any of the five old change logs; the id column names the record kind.</summary>
internal sealed record LegacyAuditRow(
    string? TransactionId,
    string? TransferId,
    string? CategoryId,
    string? CounterpartyId,
    string? AccountId,
    string? Actor,
    string? At,
    string? Action,
    string? BeforeJson,
    string? AfterJson,
    string? SnapshotJson);

internal sealed record LegacySettingRow(string? Key, string? Value);
