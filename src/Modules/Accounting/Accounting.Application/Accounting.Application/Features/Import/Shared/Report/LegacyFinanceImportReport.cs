using System.Text.Json.Serialization;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;

namespace Accounting.Application.Features.Import.Shared.Report;

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
