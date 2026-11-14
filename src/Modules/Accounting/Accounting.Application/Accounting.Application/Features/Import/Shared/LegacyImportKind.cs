using System.Text.Json.Serialization;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;

namespace Accounting.Application.Features.Import.Shared;

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
