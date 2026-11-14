using System.Text.Json.Serialization;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;

namespace Accounting.Application.Features.Import.Shared;

/// <summary>
/// How the old clients and suppliers used in operations are linked: to «Клієнти»/«Постачальники» of Brevi, or kept as
/// finance counterparties when Brevi does not know them. <c>Unused</c> ones (no operation) are not moved.
/// </summary>
public sealed record LegacyImportPartiesResponse(int ClientsLinked, int ClientsAsFinancial, int SuppliersLinked,
    int SuppliersAsFinancial, int Unused);
