using Ardalis.Result;
using Crm.Application.Files.Contracts;
using Crm.Application.Files.Reading;
using Mediator;

namespace Crm.Application.Files.GetFiles;

public sealed class GetCustomFilesQueryHandler(ICustomFileReader files)
    : IQueryHandler<GetCustomFilesQuery, Result<IReadOnlyList<PrivateFileView>>>
{
    public async ValueTask<Result<IReadOnlyList<PrivateFileView>>> Handle(GetCustomFilesQuery query, CancellationToken ct)
        => await files.ListAsync(query.DraftId, query.VersionId, query.QuoteId, query.Actor, ct);
}
