using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Accounts.Shared;
using Accounting.Application.Features.Balances.Shared;

namespace Accounting.Application.Features.Accounts.Commands.UpdateFinanceAccount;

public sealed class UpdateFinanceAccountValidator : AbstractValidator<UpdateFinanceAccountCommand>
{
    public UpdateFinanceAccountValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Fields).NotNull().SetValidator(new FinanceAccountFieldsValidator());
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).NotEmpty().MaximumLength(FinanceAccount.ByMaxLength);
    }
}
