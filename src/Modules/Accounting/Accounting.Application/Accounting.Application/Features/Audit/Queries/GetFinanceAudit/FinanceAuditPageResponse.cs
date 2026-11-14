using Accounting.Domain.Audit;

namespace Accounting.Application.Features.Audit.Queries.GetFinanceAudit;

/// <summary>A page of «Журнал змін», newest first.</summary>
public sealed record FinanceAuditPageResponse(IReadOnlyList<FinanceAuditEntryResponse> Items, int Total, int Page,
    int PageSize);
