using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Mediator;

namespace Crm.Application.CustomSewing.Calculate;

public sealed record CalculateCustomSewingCommand(Guid DraftId, long ExpectedRevision, Guid RequestId,
    CrmActor Actor) : ICommand<Result<CustomDetail>>;
