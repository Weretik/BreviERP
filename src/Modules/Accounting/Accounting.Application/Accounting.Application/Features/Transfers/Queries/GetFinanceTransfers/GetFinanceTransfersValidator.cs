using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transfers;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transfers.Shared;

namespace Accounting.Application.Features.Transfers.Queries.GetFinanceTransfers;

public sealed class GetFinanceTransfersValidator : AbstractValidator<GetFinanceTransfersQuery>
{
    public GetFinanceTransfersValidator()
    {
        RuleFor(x => x.Filter.Scope).IsInEnum();
        RuleFor(x => x.Filter.Source).IsInEnum().When(x => x.Filter.Source is not null);
        RuleFor(x => x.Filter.Search).MaximumLength(FinanceJournal.SearchMaxLength);
        RuleFor(x => x.Filter.To).GreaterThanOrEqualTo(x => x.Filter.From)
            .When(x => x.Filter.From is not null && x.Filter.To is not null)
            .WithMessage("Кінець періоду раніший за початок.");
        RuleFor(x => x.Sort).IsInEnum();
        RuleFor(x => x.Direction).IsInEnum();
        RuleFor(x => x.Page).GreaterThan(0);
    }
}
