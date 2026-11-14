using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;

namespace Accounting.Application.Features.Payments.Queries.GetFinanceTransactionsByIds;

public sealed class GetFinanceTransactionsByIdsValidator : AbstractValidator<GetFinanceTransactionsByIdsQuery>
{
    public GetFinanceTransactionsByIdsValidator()
    {
        RuleFor(x => x.Ids).NotNull();
        RuleForEach(x => x.Ids).NotEmpty();
    }
}
