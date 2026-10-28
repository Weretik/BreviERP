using Ardalis.Result;
using Crm.Application.Abstractions.Files;
using Crm.Application.Customers.Access;
using Mediator;

namespace Crm.Application.Files.GetContent;

public sealed record GetPrivateFileContentQuery(Guid FileId, Guid? VersionId, Guid? QuoteId, CrmActor Actor)
    : IQuery<Result<PrivateFileContent>>;
