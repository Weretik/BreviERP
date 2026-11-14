using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Ledger;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Transactions.Commands.CreateFinanceTransaction;

public sealed class CreateFinanceTransactionValidator : AbstractValidator<CreateFinanceTransactionCommand>
{
    public CreateFinanceTransactionValidator()
    {
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.Fields).NotNull().SetValidator(new FinanceTransactionFieldsValidator());
        RuleFor(x => x.By).ValidBy();
    }
}
