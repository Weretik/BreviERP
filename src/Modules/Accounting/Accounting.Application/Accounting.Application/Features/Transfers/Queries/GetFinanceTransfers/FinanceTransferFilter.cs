using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transfers;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transfers.Shared;

namespace Accounting.Application.Features.Transfers.Queries.GetFinanceTransfers;

/// <summary>
/// Filters of «Перекази»: dates inclusive, <c>AccountId</c> matches either side, <c>Search</c> looks in the note,
/// amounts are kopecks. Without <c>IncludeVoided</c> only active transfers are listed.
/// </summary>
public sealed record FinanceTransferFilter(
    LedgerScope Scope,
    DateOnly? From,
    DateOnly? To,
    Guid? AccountId,
    string? Search,
    long? MinMinor,
    long? MaxMinor,
    MovementSource? Source,
    bool IncludeVoided);
