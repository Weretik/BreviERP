using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Transactions.Queries.GetFinanceTransactions;

/// <summary>
/// Filters of «Операції». Dates are inclusive; <c>CategoryId</c> takes a root with its subcategories,
/// <c>SubcategoryId</c> one exact category; <c>Search</c> looks in the description, the note and the one-off name;
/// amounts are kopecks. Without <c>IncludeVoided</c> only active operations are listed. <c>NeedsReview</c> keeps the
/// operations of «Потребують перевірки»; <c>Counterparty</c> keeps those with exactly that counterparty.
/// </summary>
public sealed record FinanceTransactionFilter(
    LedgerScope Scope,
    DateOnly? From,
    DateOnly? To,
    Guid? AccountId,
    CategoryDirection? Type,
    Guid? CategoryId,
    Guid? SubcategoryId,
    string? Search,
    long? MinMinor,
    long? MaxMinor,
    MovementSource? Source,
    bool IncludeVoided,
    bool NeedsReview = false,
    CounterpartyLink? Counterparty = null);
