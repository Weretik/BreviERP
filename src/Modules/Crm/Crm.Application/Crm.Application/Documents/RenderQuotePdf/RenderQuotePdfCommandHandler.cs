using Ardalis.Result;
using Crm.Application.Documents.Artifacts;
using Crm.Application.Documents.Source;
using Mediator;

namespace Crm.Application.Documents.RenderQuotePdf;

public sealed class RenderQuotePdfCommandHandler(IQuoteDocumentSource source, IQuotePdfArtifactStore artifacts)
    : ICommandHandler<RenderQuotePdfCommand, Result<QuotePdfArtifactDetail>>
{
    public async ValueTask<Result<QuotePdfArtifactDetail>> Handle(RenderQuotePdfCommand command,
        CancellationToken cancellationToken)
    {
        if (command.QuoteId == Guid.Empty || command.VersionId == Guid.Empty || command.Actor.UserId <= 0)
            return Result.NotFound();
        var issue = await source.FindAuthorizedIssueAsync(command.QuoteId, command.VersionId,
            command.Actor, cancellationToken);
        if (issue is null) return Result.NotFound();
        var queued = await artifacts.QueueAsync(issue.QuoteId, issue.QuoteVersionId,
            issue.IssuerVersionId, issue.TemplateVersionId, "standard-v1", cancellationToken);
        if (!queued.IsSuccess) return PreserveFailure(queued);
        var metadata = queued.Value;
        if (metadata.Status == "failed")
        {
            var retried = await artifacts.RetryAsync(metadata.PublicId, cancellationToken);
            if (!retried.IsSuccess) return PreserveFailure(retried);
            metadata = retried.Value;
        }
        return Result.Success(QuotePdfArtifactDetail.From(metadata));
    }

    private static Result<QuotePdfArtifactDetail> PreserveFailure(Result<QuotePdfArtifactMetadata> result)
    {
        return result.Status switch
        {
            ResultStatus.Invalid => Result<QuotePdfArtifactDetail>.Invalid(result.ValidationErrors),
            ResultStatus.Conflict => Result<QuotePdfArtifactDetail>.Conflict(result.Errors.ToArray()),
            ResultStatus.NotFound => Result<QuotePdfArtifactDetail>.NotFound(),
            _ => Result<QuotePdfArtifactDetail>.Error(string.Join(",", result.Errors))
        };
    }
}
