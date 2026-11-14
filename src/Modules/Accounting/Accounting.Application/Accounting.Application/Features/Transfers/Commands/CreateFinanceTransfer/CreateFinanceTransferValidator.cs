using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Ledger;
using Accounting.Domain.Transfers;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transfers.Shared;

namespace Accounting.Application.Features.Transfers.Commands.CreateFinanceTransfer;

public sealed class CreateFinanceTransferValidator : AbstractValidator<CreateFinanceTransferCommand>
{
    public CreateFinanceTransferValidator()
    {
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.Fields).NotNull().SetValidator(new FinanceTransferFieldsValidator());
        RuleFor(x => x.By).ValidBy();
    }
}
