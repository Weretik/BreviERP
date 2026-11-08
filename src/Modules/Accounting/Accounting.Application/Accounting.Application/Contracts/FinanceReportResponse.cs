using System.Text.Json.Serialization;
using Accounting.Domain.Categories;
using Accounting.Domain.Movements;

namespace Accounting.Application.Contracts;

/// <summary>What a report of «Звіти» groups by.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<FinanceReportKind>))]
public enum FinanceReportKind
{
    /// <summary>Income, expense and the difference per currency.</summary>
    Summary,

    /// <summary>Expenses by root category with its subcategories.</summary>
    ExpenseStructure,

    /// <summary>Income and expense by category and subcategory.</summary>
    Categories,

    /// <summary>Movement of each account: income, expense and transfers in and out.</summary>
    Accounts,

    /// <summary>Income and expense by counterparty.</summary>
    Counterparties,

    /// <summary>The operations themselves, oldest first.</summary>
    Operations,
}

/// <summary>A row of a report: a group total, a subcategory inside its category or one operation.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<FinanceReportRowType>))]
public enum FinanceReportRowType
{
    Group,
    Subcategory,
    Operation,
}

/// <summary>
/// A built report: totals per currency (only income and expense, never transfers) and its rows. The operations report
/// returns at most a limited number of rows (<c>Limited</c>), with <c>TotalRows</c> the full count.
/// </summary>
public sealed record FinanceReportResponse(
    FinanceReportKind Kind,
    DateOnly From,
    DateOnly To,
    IReadOnlyList<FinanceReportTotalResponse> Totals,
    IReadOnlyList<FinanceReportRowResponse> Rows,
    int TotalRows,
    bool Limited);

public sealed record FinanceReportTotalResponse(string Currency, int Operations, long IncomeMinor, long ExpenseMinor,
    long ResultMinor);

/// <summary>
/// One row; a field the report kind does not use is null. Sums are kopecks. A category row of the expense structure
/// already holds its subcategories, so the rows must not be added up together.
/// </summary>
public sealed record FinanceReportRowResponse(
    FinanceReportRowType RowType,
    string Currency,
    int Operations,
    long IncomeMinor,
    long ExpenseMinor,
    long ResultMinor,
    string? Account = null,
    CategoryDirection? Type = null,
    string? Category = null,
    string? Subcategory = null,
    string? Counterparty = null,
    long? IncomingTransfersMinor = null,
    long? OutgoingTransfersMinor = null,
    DateOnly? Date = null,
    long? AmountMinor = null,
    string? Note = null,
    MovementSource? Source = null,
    Guid? Id = null);
