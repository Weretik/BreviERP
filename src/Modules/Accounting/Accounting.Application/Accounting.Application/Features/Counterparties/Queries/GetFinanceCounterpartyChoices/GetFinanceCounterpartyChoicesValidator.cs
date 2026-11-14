using Accounting.Application.Abstractions;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties;
using Accounting.Application.Features.Journal.Shared;

namespace Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterpartyChoices;

public sealed class GetFinanceCounterpartyChoicesValidator : AbstractValidator<GetFinanceCounterpartyChoicesQuery>
{
    public GetFinanceCounterpartyChoicesValidator() =>
        RuleFor(x => x.Search).MaximumLength(FinanceJournal.SearchMaxLength);
}
