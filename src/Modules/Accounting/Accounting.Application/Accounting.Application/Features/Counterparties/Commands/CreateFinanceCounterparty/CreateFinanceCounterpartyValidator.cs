using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Counterparties;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;

namespace Accounting.Application.Features.Counterparties.Commands.CreateFinanceCounterparty;

public sealed class CreateFinanceCounterpartyValidator : AbstractValidator<CreateFinanceCounterpartyCommand>
{
    public CreateFinanceCounterpartyValidator()
    {
        RuleFor(x => x.Fields).NotNull().SetValidator(new FinanceCounterpartyFieldsValidator());
        RuleFor(x => x.By).ValidBy();
    }
}
