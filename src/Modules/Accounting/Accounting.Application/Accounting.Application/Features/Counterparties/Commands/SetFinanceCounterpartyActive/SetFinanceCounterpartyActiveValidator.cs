using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;

namespace Accounting.Application.Features.Counterparties.Commands.SetFinanceCounterpartyActive;

public sealed class SetFinanceCounterpartyActiveValidator : AbstractValidator<SetFinanceCounterpartyActiveCommand>
{
    public SetFinanceCounterpartyActiveValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}
