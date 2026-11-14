using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Attachments;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Transactions;
using Accounting.Domain.Transfers;
using Accounting.Application.Features.Accounts.Shared;
using Accounting.Application.Features.Attachments.Shared;
using Accounting.Application.Features.Categories.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;
using Accounting.Application.Features.Transfers.Shared;

namespace Accounting.Application.Features.Audit.Shared;

/// <summary>Writes «Журнал змін» entries with JSON snapshots of the API shape of the record.</summary>
internal static class AuditTrail
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        // Ukrainian text stays readable in the log.
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    public static string Snapshot(FinanceAccount account) => JsonSerializer.Serialize(FinanceAccountResponse.From(account, null), Json);

    public static string Snapshot(FinanceCategory category) =>
        JsonSerializer.Serialize(FinanceCategoryResponse.From(category), Json);

    public static string Snapshot(FinanceTransaction transaction, FinanceNames names) =>
        JsonSerializer.Serialize(FinanceTransactionResponse.From(transaction, names), Json);

    public static string Snapshot(FinanceTransfer transfer, FinanceNames names) =>
        JsonSerializer.Serialize(FinanceTransferResponse.From(transfer, names), Json);

    public static string Snapshot(FinanceCounterparty counterparty, FinanceNames names) =>
        JsonSerializer.Serialize(FinanceCounterpartyResponse.From(counterparty, names), Json);

    public static string Snapshot(FinanceAttachment attachment) =>
        JsonSerializer.Serialize(FinanceAttachmentResponse.From(attachment), Json);

    public static void Add(IAccountingDbContext db, AuditEntity entity, Guid entityId, AuditAction action, string by,
        DateTimeOffset at, string? before, string? after) =>
        db.AuditEntries.Add(new AuditEntry(entity, entityId, action, by, at, before, after));
}
