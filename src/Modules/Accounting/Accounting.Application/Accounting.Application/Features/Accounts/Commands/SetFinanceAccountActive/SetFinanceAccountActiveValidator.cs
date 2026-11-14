using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Accounts.Shared;

namespace Accounting.Application.Features.Accounts.Commands.SetFinanceAccountActive;

public sealed class SetFinanceAccountActiveValidator : AbstractValidator<SetFinanceAccountActiveCommand>
{
    public SetFinanceAccountActiveValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).NotEmpty().MaximumLength(FinanceAccount.ByMaxLength);
    }
}
