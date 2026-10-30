using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Documents.Artifacts;
using Crm.Application.Documents.RenderQuotePdf;
using Crm.Application.Documents.Source;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Issues;
using Moq;

namespace UnitTests.Crm.Documents;

public sealed class RenderQuotePdfCommandHandlerTests
{
    [Test]
    public async Task Invalid_queue_result_keeps_validation_errors()
    {
        var (source, artifacts, command, issue) = Arrange();
        artifacts.Setup(x => x.QueueAsync(issue.QuoteId, issue.QuoteVersionId,
                issue.IssuerVersionId, issue.TemplateVersionId, "standard-v1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<QuotePdfArtifactMetadata>.Invalid(
                new ValidationError("artifact", "invalid_artifact_identity")));
        var result = await new RenderQuotePdfCommandHandler(source.Object, artifacts.Object)
            .Handle(command, default);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(result.ValidationErrors.Select(x => x.ErrorMessage),
            Does.Contain("invalid_artifact_identity"));
    }

    [Test]
    public async Task Retry_conflict_stays_conflict()
    {
        var (source, artifacts, command, issue) = Arrange();
        var now = DateTimeOffset.UtcNow;
        var metadata = new QuotePdfArtifactMetadata(Guid.NewGuid(), issue.QuoteId,
            issue.QuoteVersionId, issue.IssuerVersionId, issue.TemplateVersionId,
            "standard-v1", "failed", 1, null, null, "render_failed", now, now, null);
        artifacts.Setup(x => x.QueueAsync(issue.QuoteId, issue.QuoteVersionId,
                issue.IssuerVersionId, issue.TemplateVersionId, "standard-v1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(metadata));
        artifacts.Setup(x => x.RetryAsync(metadata.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<QuotePdfArtifactMetadata>.Conflict("artifact_retry_conflict"));

        var result = await new RenderQuotePdfCommandHandler(source.Object, artifacts.Object)
            .Handle(command, default);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(result.Errors, Does.Contain("artifact_retry_conflict"));
    }

    [Test]
    public async Task Missing_queue_result_stays_not_found()
    {
        var (source, artifacts, command, issue) = Arrange();
        artifacts.Setup(x => x.QueueAsync(issue.QuoteId, issue.QuoteVersionId,
                issue.IssuerVersionId, issue.TemplateVersionId, "standard-v1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<QuotePdfArtifactMetadata>.NotFound());

        var result = await new RenderQuotePdfCommandHandler(source.Object, artifacts.Object)
            .Handle(command, default);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
    }

    private static (Mock<IQuoteDocumentSource> Source, Mock<IQuotePdfArtifactStore> Artifacts,
        RenderQuotePdfCommand Command, QuoteIssue Issue) Arrange()
    {
        var quoteId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var issue = QuoteIssue.Create(new QuoteId(1), 2,
            new IssuerSettingsVersionId(3), new DocumentTemplateVersionId(4),
            "10/10/2026", DateTimeOffset.UtcNow);
        var source = new Mock<IQuoteDocumentSource>(MockBehavior.Strict);
        source.Setup(x => x.FindAuthorizedIssueAsync(quoteId, versionId,
                It.IsAny<CrmActor>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(issue);
        return (source, new Mock<IQuotePdfArtifactStore>(MockBehavior.Strict),
            new RenderQuotePdfCommand(quoteId, versionId, new CrmActor(7, true)), issue);
    }
}
