using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Mediator;

namespace Crm.Application.CustomSewing.SaveVersion;

public sealed record SaveCustomVersionCommand(Guid DraftId, long ExpectedRevision, Guid RequestId,
    Guid BasisToken, CrmActor Actor) : ICommand<Result<CustomDetail>>;
