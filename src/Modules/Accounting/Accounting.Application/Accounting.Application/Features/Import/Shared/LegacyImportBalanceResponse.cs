using System.Text.Json.Serialization;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;

namespace Accounting.Application.Features.Import.Shared;

/// <summary>
/// An account after the move, counted from the file: <c>ArchiveMinor</c> — balance on 30.09.2026 (opening plus active
/// archive movements); <c>CurrentMinor</c> — today (confirmed balance on 01.10 plus the current books), null while that
/// balance is not confirmed; <c>DifferenceMinor</c> — confirmed balance minus the archive one when they differ.
/// </summary>
public sealed record LegacyImportBalanceResponse(Guid AccountId, string Name, string Currency, bool Active,
    long ArchiveMinor, long? CutoverMinor, long? CurrentMinor, long? DifferenceMinor);
