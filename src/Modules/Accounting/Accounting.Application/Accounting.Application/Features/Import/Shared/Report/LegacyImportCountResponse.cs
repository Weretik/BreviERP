using System.Text.Json.Serialization;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Application.Features.Import.Shared.Reading;

namespace Accounting.Application.Features.Import.Shared.Report;

/// <summary>Records of one kind: in the file, to add, already moved earlier (same id), left out on purpose.</summary>
public sealed record LegacyImportCountResponse(LegacyImportKind Kind, int InFile, int ToAdd, int AlreadyMoved, int Skipped);
