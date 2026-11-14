using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;

namespace Accounting.Application.Features.Counterparties.Commands.VerifyFinanceCounterparty;

public sealed class VerifyFinanceCounterpartyValidator : AbstractValidator<VerifyFinanceCounterpartyCommand>
{
    public VerifyFinanceCounterpartyValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}
