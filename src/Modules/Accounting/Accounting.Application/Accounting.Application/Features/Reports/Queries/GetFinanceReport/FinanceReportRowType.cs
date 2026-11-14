using System.Text.Json.Serialization;
using Accounting.Domain.Categories;
using Accounting.Domain.Movements;

namespace Accounting.Application.Features.Reports.Queries.GetFinanceReport;

/// <summary>A row of a report: a group total, a subcategory inside its category or one operation.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<FinanceReportRowType>))]
public enum FinanceReportRowType
{
    Group,
    Subcategory,
    Operation,
}
