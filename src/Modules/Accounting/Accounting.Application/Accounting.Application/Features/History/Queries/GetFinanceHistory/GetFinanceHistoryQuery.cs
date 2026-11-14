using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;

namespace Accounting.Application.Features.History.Queries.GetFinanceHistory;

/// <summary>«Історія» of one record: its log entries, newest first.</summary>
public sealed record GetFinanceHistoryQuery(AuditEntity Entity, Guid Id) : IQuery<Result<IReadOnlyList<FinanceHistoryEntryResponse>>>;
