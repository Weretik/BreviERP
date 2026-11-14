using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Balances.Shared;

namespace Accounting.Application.Features.Overview.Queries.GetFinanceOverview;

public sealed class GetFinanceOverviewValidator : AbstractValidator<GetFinanceOverviewQuery>
{
    public GetFinanceOverviewValidator()
    {
        RuleFor(x => x.Scope).IsInEnum().When(x => x.Scope is not null);
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage("Кінець періоду раніший за початок.");
    }
}
