using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Files.Contracts;
using Mediator;

namespace Crm.Application.Files.Upload;

public sealed record UploadPrivateFileCommand(Guid DraftId, Guid RequestId, Stream Content,
    string ContentType, CrmActor Actor) : ICommand<Result<PrivateFileView>>;
