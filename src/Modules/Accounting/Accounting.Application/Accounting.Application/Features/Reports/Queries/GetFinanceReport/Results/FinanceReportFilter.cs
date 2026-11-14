using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Reports.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Reports.Queries.GetFinanceReport.Results;

/// <summary>
/// The settings of a report of «Звіти». Dates are inclusive; <c>Scope</c> null takes both areas; <c>CategoryId</c> takes a
/// root with its subcategories (or one subcategory). Only active income and expense are counted. The accounts report
/// adds the transfers of the period, area and account; the type, category and counterparty do not filter them.
/// </summary>
public sealed record FinanceReportFilter(
    FinanceReportKind Kind,
    DateOnly From,
    DateOnly To,
    LedgerScope? Scope = null,
    Guid? AccountId = null,
    CategoryDirection? Type = null,
    Guid? CategoryId = null,
    FinanceCounterpartyLinkInput? Counterparty = null);
