using Accounting.Application.Abstractions;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties;

public sealed class GetFinanceCounterpartiesValidator : AbstractValidator<GetFinanceCounterpartiesQuery>
{
    public GetFinanceCounterpartiesValidator()
    {
        RuleFor(x => x.Tab).IsInEnum();
        RuleFor(x => x.Search).MaximumLength(FinanceJournal.SearchMaxLength);
        RuleFor(x => x.Page).InclusiveBetween(1, 100_000);
    }
}
