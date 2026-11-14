using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Accounts.Shared;

namespace Accounting.Application.Features.Accounts.Commands.SetCutoverBalance;

public sealed class SetCutoverBalanceValidator : AbstractValidator<SetCutoverBalanceCommand>
{
    public SetCutoverBalanceValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.BalanceMinor).NotNull().When(x => x.Confirmed).WithMessage("Вкажіть залишок на 01.10.2026.");
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).NotEmpty().MaximumLength(FinanceAccount.ByMaxLength);
    }
}
