using System.Text.Json.Serialization;
using Accounting.Domain.Categories;
using Accounting.Domain.Movements;

namespace Accounting.Application.Features.Reports.Shared;

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
