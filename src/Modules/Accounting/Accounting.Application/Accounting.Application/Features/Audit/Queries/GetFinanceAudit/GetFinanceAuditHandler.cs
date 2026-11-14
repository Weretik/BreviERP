using System.Globalization;
using System.Text.Json;
using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Reports.Shared;

namespace Accounting.Application.Features.Audit.Queries.GetFinanceAudit;

public sealed class GetFinanceAuditHandler(IAccountingDbContext db)
    : IQueryHandler<GetFinanceAuditQuery, Result<FinanceAuditPageResponse>>
{
    public async ValueTask<Result<FinanceAuditPageResponse>> Handle(GetFinanceAuditQuery query,
        CancellationToken cancellationToken)
    {
        var entries = db.AuditEntries.AsNoTracking();
        if (query.Entity is { } entity) entries = entries.Where(x => x.Entity == entity);
        if (query.Action is { } action) entries = entries.Where(x => x.Action == action);
        var total = await entries.CountAsync(cancellationToken);
        var page = await entries.OrderByDescending(x => x.Id)
            .Skip((query.Page - 1) * FinanceJournal.PageSize).Take(FinanceJournal.PageSize)
            .ToListAsync(cancellationToken);
        return Result.Success(new FinanceAuditPageResponse(
            page.Select(x => new FinanceAuditEntryResponse(x.Id, x.Entity, x.EntityId, x.Action, x.By, x.At,
                AuditTitle.Of(x.AfterJson ?? x.BeforeJson), x.BeforeJson, x.AfterJson)).ToList(),
            total, query.Page, FinanceJournal.PageSize));
    }
}
