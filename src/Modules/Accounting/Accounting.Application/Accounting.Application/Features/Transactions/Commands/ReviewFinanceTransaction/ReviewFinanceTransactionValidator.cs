using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Transactions;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Transactions.Commands.ReviewFinanceTransaction;

public sealed class ReviewFinanceTransactionValidator : AbstractValidator<ReviewFinanceTransactionCommand>
{
    public ReviewFinanceTransactionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Оберіть категорію.");
        RuleFor(x => x.Counterparty!).SetValidator(new FinanceCounterpartyLinkInputValidator()).When(x => x.Counterparty is not null);
        RuleFor(x => x.OneoffName).MaximumLength(FinanceTransactionDetails.OneoffNameMaxLength + 20);
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}
