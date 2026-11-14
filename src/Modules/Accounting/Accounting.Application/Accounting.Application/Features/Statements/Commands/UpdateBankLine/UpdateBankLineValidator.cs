using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Statements;
using Accounting.Application;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Statements.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Statements.Commands.UpdateBankLine;

public sealed class UpdateBankLineValidator : AbstractValidator<UpdateBankLineCommand>
{
    public UpdateBankLineValidator()
    {
        RuleFor(x => x.StatementId).NotEmpty();
        RuleFor(x => x.LineId).NotEmpty();
        RuleFor(x => x.Counterparty!).SetValidator(new FinanceCounterpartyLinkInputValidator()).When(x => x.Counterparty is not null);
    }
}
