using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Accounts.Shared;
using Accounting.Application.Features.Audit.Shared;

namespace Accounting.Application.Features.Accounts.Commands.CreateFinanceAccount;

public sealed class CreateFinanceAccountValidator : AbstractValidator<CreateFinanceAccountCommand>
{
    public CreateFinanceAccountValidator()
    {
        RuleFor(x => x.Fields).NotNull().SetValidator(new FinanceAccountFieldsValidator());
        RuleFor(x => x.By).NotEmpty().MaximumLength(FinanceAccount.ByMaxLength);
    }
}
