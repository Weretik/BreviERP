using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Mediator;
using Crm.Domain.CustomSewing.Composition;

namespace Crm.Application.CustomSewing.SaveDraft;

public sealed record SaveCustomDraftCommand(Guid DraftId, long ExpectedRevision, Guid RequestId,
    CustomConfiguration Configuration, CrmActor Actor) : ICommand<Result<CustomDetail>>;
