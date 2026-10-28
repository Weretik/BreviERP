using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Files.Contracts;
using Mediator;

namespace Crm.Application.Files.GetFiles;

public sealed record GetCustomFilesQuery(Guid DraftId, Guid? VersionId, Guid? QuoteId, CrmActor Actor)
    : IQuery<Result<IReadOnlyList<PrivateFileView>>>;
