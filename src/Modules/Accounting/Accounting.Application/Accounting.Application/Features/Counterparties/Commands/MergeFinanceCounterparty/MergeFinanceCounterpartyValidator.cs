using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;

namespace Accounting.Application.Features.Counterparties.Commands.MergeFinanceCounterparty;

public sealed class MergeFinanceCounterpartyValidator : AbstractValidator<MergeFinanceCounterpartyCommand>
{
    public MergeFinanceCounterpartyValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TargetId).NotEmpty().NotEqual(x => x.Id).WithMessage("Оберіть іншого контрагента.");
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.TargetVersion).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}
