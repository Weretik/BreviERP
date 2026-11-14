using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Transactions.Queries.GetFinanceTransactions;

public sealed class GetFinanceTransactionsValidator : AbstractValidator<GetFinanceTransactionsQuery>
{
    public GetFinanceTransactionsValidator()
    {
        RuleFor(x => x.Filter.Scope).IsInEnum();
        RuleFor(x => x.Filter.Type).IsInEnum().When(x => x.Filter.Type is not null);
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
