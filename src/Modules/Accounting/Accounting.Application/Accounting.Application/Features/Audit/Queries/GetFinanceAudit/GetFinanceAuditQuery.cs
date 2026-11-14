using System.Globalization;
using System.Text.Json;
using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Reports.Shared;

namespace Accounting.Application.Features.Audit.Queries.GetFinanceAudit;

/// <summary>«Журнал змін»: changes of all finance records, newest first, 50 per page; optionally of one kind or action.</summary>
public sealed record GetFinanceAuditQuery(AuditEntity? Entity, AuditAction? Action, int Page)
    : IQuery<Result<FinanceAuditPageResponse>>;
