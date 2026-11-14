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
using Accounting.Application.Features.Reports.Queries.GetFinanceReport.Results;

namespace Accounting.Application.Features.Reports.Queries.GetFinanceReport;

public sealed class GetFinanceReportValidator : AbstractValidator<GetFinanceReportQuery>
{
    public GetFinanceReportValidator()
    {
        RuleFor(x => x.Filter.Kind).IsInEnum();
        RuleFor(x => x.Filter.Scope).IsInEnum().When(x => x.Filter.Scope is not null);
        RuleFor(x => x.Filter.Type).IsInEnum().When(x => x.Filter.Type is not null);
        RuleFor(x => x.Filter.To).GreaterThanOrEqualTo(x => x.Filter.From)
            .WithMessage("Кінець періоду раніший за початок.");
        RuleFor(x => x.Filter.Counterparty!).SetValidator(new FinanceCounterpartyLinkInputValidator())
            .When(x => x.Filter.Counterparty is not null);
        RuleFor(x => x.RowLimit).InclusiveBetween(1, FinanceReports.CsvRows);
    }
}
