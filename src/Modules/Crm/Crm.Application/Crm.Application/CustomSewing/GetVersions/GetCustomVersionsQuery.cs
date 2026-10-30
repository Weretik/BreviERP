using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Mediator;

namespace Crm.Application.CustomSewing.GetVersions;

public sealed record GetCustomVersionsQuery(Guid DraftId, int Page, int PageSize, CrmActor Actor) : IQuery<PagedResult<IReadOnlyList<CustomVersionSummary>>>;
