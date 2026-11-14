using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;

namespace Accounting.Application.Features.Payments.Queries.GetFinancePaymentCandidates;

public sealed class GetFinancePaymentCandidatesValidator : AbstractValidator<GetFinancePaymentCandidatesQuery>
{
    public GetFinancePaymentCandidatesValidator()
    {
        RuleFor(x => x.SupplierId).GreaterThan(0).When(x => x.SupplierId is not null);
        RuleFor(x => x.AmountMinor).GreaterThan(0).When(x => x.AmountMinor is not null);
        RuleFor(x => x.Search).MaximumLength(GetFinancePaymentCandidatesQuery.SearchMaxLength);
    }
}
