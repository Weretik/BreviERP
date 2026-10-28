using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Crm.Application.Files.Contracts;
using Mediator;

namespace Crm.Application.Files.Finalize;

public sealed record SetCustomFilesCommand(Guid DraftId, Guid RequestId, long ExpectedRevision,
    IReadOnlyList<CustomFileSelection> Files, CrmActor Actor) : ICommand<Result<CustomDetail>>;
