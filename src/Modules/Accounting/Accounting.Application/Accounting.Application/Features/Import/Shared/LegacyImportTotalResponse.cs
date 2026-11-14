using System.Text.Json.Serialization;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;

namespace Accounting.Application.Features.Import.Shared;

/// <summary>Movements of the file by area, kind and state, in kopecks.</summary>
public sealed record LegacyImportTotalResponse(LedgerScope Scope, LegacyImportMovement Movement, MovementStatus Status,
    string Currency, int Count, long AmountMinor);
