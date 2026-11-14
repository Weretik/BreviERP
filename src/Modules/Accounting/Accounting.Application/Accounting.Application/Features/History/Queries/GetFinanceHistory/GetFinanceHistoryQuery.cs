using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;

namespace Accounting.Application.Features.History.Queries.GetFinanceHistory;

/// <summary>«Історія» of one record: its log entries, newest first.</summary>
public sealed record GetFinanceHistoryQuery(AuditEntity Entity, Guid Id) : IQuery<Result<IReadOnlyList<FinanceHistoryEntryResponse>>>;

public sealed class GetFinanceHistoryValidator : AbstractValidator<GetFinanceHistoryQuery>
{
    public GetFinanceHistoryValidator()
    {
        RuleFor(x => x.Entity).IsInEnum();
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class GetFinanceHistoryHandler(IAccountingDbContext db)
    : IQueryHandler<GetFinanceHistoryQuery, Result<IReadOnlyList<FinanceHistoryEntryResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<FinanceHistoryEntryResponse>>> Handle(GetFinanceHistoryQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<FinanceHistoryEntryResponse> rows = await db.AuditEntries.AsNoTracking()
            .Where(x => x.Entity == query.Entity && x.EntityId == query.Id)
            .OrderByDescending(x => x.Id)
            .Select(x => new FinanceHistoryEntryResponse(x.Id, x.Entity, x.EntityId, x.Action, x.By, x.At, x.BeforeJson,
                x.AfterJson))
            .ToListAsync(cancellationToken);
        return Result.Success(rows);
    }
}
