using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Mediator;

namespace Crm.Application.CustomSewing.GetDraft;

public sealed record GetCustomDraftQuery(Guid DraftId, CrmActor Actor) : IQuery<Result<CustomDetail>>;
