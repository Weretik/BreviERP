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

namespace Accounting.Application.Features.Reports.Queries.GetFinanceReport;

public static class FinanceReports
{
    /// <summary>Operations shown on the screen.</summary>
    public const int ScreenRows = 100;

    /// <summary>Operations written to a CSV file; a longer journal is split by dates.</summary>
    public const int CsvRows = 50_000;

    public const string WithoutCategory = "Без категорії";
    public const string WithoutSubcategory = "Без підкатегорії";
    public const string WithoutCounterparty = "Без контрагента";
    public const string UnknownCounterparty = "Контрагента не знайдено";
}
