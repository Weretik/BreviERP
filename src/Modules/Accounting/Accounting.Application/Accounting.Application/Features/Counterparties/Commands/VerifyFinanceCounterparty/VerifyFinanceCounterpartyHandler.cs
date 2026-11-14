using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;

namespace Accounting.Application.Features.Counterparties.Commands.VerifyFinanceCounterparty;

public sealed class VerifyFinanceCounterpartyHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<VerifyFinanceCounterpartyCommand, Result<FinanceCounterpartyResponse>>
{
    public async ValueTask<Result<FinanceCounterpartyResponse>> Handle(VerifyFinanceCounterpartyCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        return await FinanceCounterpartyChange.ApplyAsync(db, command.Id, command.Version, command.By, now,
            async counterparty =>
            {
                var verified = counterparty.Verify(now);
                if (verified.IsFailure) return verified.Error!;
                if (!verified.Value) return DomainResult<AuditAction?>.Success(null);
                foreach (var operation in await FinanceCounterpartyChange.OperationsOfAsync(db, counterparty, cancellationToken))
                    operation.FollowCounterparty(counterparty.State);
                return DomainResult<AuditAction?>.Success(AuditAction.Verify);
            },
            cancellationToken);
    }
}
