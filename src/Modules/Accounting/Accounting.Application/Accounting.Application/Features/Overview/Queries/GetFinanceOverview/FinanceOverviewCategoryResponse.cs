using Accounting.Domain.Ledger;

namespace Accounting.Application.Features.Overview.Queries.GetFinanceOverview;

/// <summary>An expense root category of «Куди пішли гроші»; <c>CategoryId</c> is null for operations without one.</summary>
public sealed record FinanceOverviewCategoryResponse(Guid? CategoryId, string Name, long AmountMinor);
