using Accounting.Application.Abstractions;
using Accounting.Application.Features.Accounts.Shared;
using Accounting.Application.Features.Balances.Shared;

namespace Accounting.Application.Features.Accounts.Queries.GetFinanceAccounts;

/// <summary>
/// «Рахунки і каси»: active accounts by order and name, with balances; archived ones too with «Показати архівні».
/// </summary>
public sealed record GetFinanceAccountsQuery(bool IncludeArchived) : IQuery<Result<IReadOnlyList<FinanceAccountResponse>>>;

public sealed class GetFinanceAccountsHandler(IAccountingDbContext db)
    : IQueryHandler<GetFinanceAccountsQuery, Result<IReadOnlyList<FinanceAccountResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<FinanceAccountResponse>>> Handle(GetFinanceAccountsQuery query,
        CancellationToken cancellationToken)
    {
        var accounts = await db.Accounts.AsNoTracking()
            .Where(x => query.IncludeArchived || x.Active)
            .ToListAsync(cancellationToken);
        var balances = await FinanceBalances.ForAsync(db, accounts, cancellationToken);
        IReadOnlyList<FinanceAccountResponse> rows = accounts
            .OrderBy(x => !x.Active).ThenBy(x => x.SortOrder).ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(x => FinanceAccountResponse.From(x, balances[x.Id])).ToList();
        return Result.Success(rows);
    }
}
