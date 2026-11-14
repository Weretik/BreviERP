using System.Text.Json.Serialization;
using Accounting.Domain.Categories;
using Accounting.Domain.Movements;
using Accounting.Application.Features.Reports.Queries.GetFinanceReport;

namespace Accounting.Application.Features.Reports.Shared;

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
