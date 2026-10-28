using Ardalis.Result;
using Crm.Application.Abstractions.Files;
using Crm.Application.Files.Reading;
using Mediator;

namespace Crm.Application.Files.GetContent;

public sealed class GetPrivateFileContentQueryHandler(ICustomFileReader files)
    : IQueryHandler<GetPrivateFileContentQuery, Result<PrivateFileContent>>
{
    public async ValueTask<Result<PrivateFileContent>> Handle(GetPrivateFileContentQuery query, CancellationToken ct)
        => await files.ContentAsync(query.FileId, query.VersionId, query.QuoteId, query.Actor, ct);
}
