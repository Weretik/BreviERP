using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Accounts.Shared;
using Accounting.Application.Features.Audit.Shared;

namespace Accounting.Application.Features.Accounts.Commands.CreateFinanceAccount;

public sealed class CreateFinanceAccountHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<CreateFinanceAccountCommand, Result<FinanceAccountResponse>>
{
    public async ValueTask<Result<FinanceAccountResponse>> Handle(CreateFinanceAccountCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var details = command.Fields.ToDetails();
        if (details.IsFailure) return details.Error!.ToResult<FinanceAccountResponse>();
        var account = FinanceAccount.Create(FinanceAccountId.New(), details.Value, now);
        db.Accounts.Add(account);
        AuditTrail.Add(db, AuditEntity.Account, account.Id.Value, AuditAction.Create, command.By, now, null,
            AuditTrail.Snapshot(account));
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(FinanceAccountResponse.From(account,
            new FinanceAccountBalanceResponse(account.OpeningMinor, null, account.OpeningMinor, false)));
    }
}
