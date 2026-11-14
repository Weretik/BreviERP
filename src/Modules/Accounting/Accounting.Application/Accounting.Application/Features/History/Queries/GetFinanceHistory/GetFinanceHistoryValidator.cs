using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;

namespace Accounting.Application.Features.History.Queries.GetFinanceHistory;

public sealed class GetFinanceHistoryValidator : AbstractValidator<GetFinanceHistoryQuery>
{
    public GetFinanceHistoryValidator()
    {
        RuleFor(x => x.Entity).IsInEnum();
        RuleFor(x => x.Id).NotEmpty();
    }
}
