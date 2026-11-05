using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;

namespace Accounting.Application.Accounts;

/// <summary>«Рахунки і каси»: active accounts by order and name; archived ones too with «Показати архівні».</summary>
public sealed record GetFinanceAccountsQuery(bool IncludeArchived) : IQuery<Result<IReadOnlyList<FinanceAccountResponse>>>;

public sealed class GetFinanceAccountsQueryHandler(IAccountingDbContext db)
    : IQueryHandler<GetFinanceAccountsQuery, Result<IReadOnlyList<FinanceAccountResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<FinanceAccountResponse>>> Handle(GetFinanceAccountsQuery query,
        CancellationToken cancellationToken)
    {
        var accounts = await db.Accounts.AsNoTracking()
            .Where(x => query.IncludeArchived || x.Active)
            .ToListAsync(cancellationToken);
        IReadOnlyList<FinanceAccountResponse> rows = accounts
            .OrderBy(x => !x.Active).ThenBy(x => x.SortOrder).ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(FinanceAccountResponse.From).ToList();
        return Result.Success(rows);
    }
}
