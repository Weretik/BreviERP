using Crm.Application.Documents.EmailTemplates;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.BuildIntroduction;
using Crm.Domain.Quotes.Introductions;

namespace Crm.Application.Quotes.Introductions;

public sealed class IntroductionTemplateService(
    IIntroductionTemplateStore templates,
    IIntroductionFeaturedMappingStore mappings,
    IntroductionDraftBuilder builder,
    IEmailTemplateVersionReader emailTemplates)
{
    public async Task<Result<IntroductionTemplateDetail>> CreateAsync(CrmActor actor, Guid requestId, string name,
        IntroductionLayout layout, Guid emailTemplateVersionId,
        IReadOnlyList<IntroductionSelection> selections, CancellationToken cancellationToken)
    {
        if (actor.UserId <= 0) return Result<IntroductionTemplateDetail>.Forbidden();
        if (requestId == Guid.Empty) return Result<IntroductionTemplateDetail>.Invalid(new ValidationError("requestId", "invalid_request"));
        if (!await emailTemplates.ExistsAsync(emailTemplateVersionId, cancellationToken))
            return Result<IntroductionTemplateDetail>.Invalid(new ValidationError("emailTemplateVersionId", "email_template_version_missing"));
        var payload = await PreparePayloadAsync(layout, emailTemplateVersionId, selections, cancellationToken);
        return payload.IsSuccess ? await templates.CreateAsync(actor, requestId, name, payload.Value, cancellationToken)
            : PayloadFailure(payload);
    }

    public async Task<Result<IntroductionTemplateDetail>> ReviseAsync(CrmActor actor, Guid requestId, Guid templatePublicId,
        long expectedRevision, string name, IntroductionLayout layout, Guid emailTemplateVersionId,
        IReadOnlyList<IntroductionSelection> selections, CancellationToken cancellationToken)
    {
        if (actor.UserId <= 0) return Result<IntroductionTemplateDetail>.Forbidden();
        if (requestId == Guid.Empty) return Result<IntroductionTemplateDetail>.Invalid(new ValidationError("requestId", "invalid_request"));
        var current = await templates.ReadAsync(actor, templatePublicId, null, cancellationToken);
        if (current is null) return Result<IntroductionTemplateDetail>.NotFound();
        // Historical local selections may retain the exact old reference; changing it requires a real registry version.
        if (current.Snapshot.EmailTemplateVersionId != emailTemplateVersionId &&
            !await emailTemplates.ExistsAsync(emailTemplateVersionId, cancellationToken))
            return Result<IntroductionTemplateDetail>.Invalid(new ValidationError("emailTemplateVersionId", "email_template_version_missing"));
        var payload = await PreparePayloadAsync(layout, emailTemplateVersionId, selections, cancellationToken);
        return payload.IsSuccess
            ? await templates.ReviseAsync(actor, requestId, templatePublicId, expectedRevision, name, payload.Value, cancellationToken)
            : PayloadFailure(payload);
    }

    public Task<Result<IntroductionTemplateDetail>> SetArchivedAsync(CrmActor actor, Guid requestId, Guid templatePublicId,
        long expectedRevision, bool archived, CancellationToken cancellationToken)
        => actor.UserId <= 0
            ? Task.FromResult(Result<IntroductionTemplateDetail>.Forbidden())
            : requestId == Guid.Empty
                ? Task.FromResult(Result<IntroductionTemplateDetail>.Invalid(new ValidationError("requestId", "invalid_request")))
                : templates.SetArchivedAsync(actor, requestId, templatePublicId, expectedRevision, archived, cancellationToken);

    public Task<IntroductionTemplateDetail?> ReadAsync(CrmActor actor, Guid templatePublicId,
        Guid? versionPublicId, CancellationToken cancellationToken)
        => templates.ReadAsync(actor, templatePublicId, versionPublicId, cancellationToken);

    public Task<PagedResult<IReadOnlyList<IntroductionTemplateSummary>>> ListAsync(CrmActor actor,
        bool includeArchived, int page, int pageSize,
        CancellationToken cancellationToken)
        => templates.ListAsync(actor, includeArchived, page, pageSize, cancellationToken);

    public async Task<Result<IntroductionPreparedDraft>> ApplyAsync(CrmActor actor, Guid templatePublicId,
        Guid? versionPublicId, CancellationToken cancellationToken)
    {
        if (actor.UserId <= 0 || templatePublicId == Guid.Empty)
            return Result<IntroductionPreparedDraft>.Forbidden();
        var detail = await templates.ReadAsync(actor, templatePublicId, versionPublicId, cancellationToken);
        if (detail is null) return Result<IntroductionPreparedDraft>.NotFound();
        if (detail.Archived) return Result<IntroductionPreparedDraft>.Conflict("template_archived");
        var mapping = await mappings.ReadActiveAsync(cancellationToken);
        if (mapping is null) return Result<IntroductionPreparedDraft>.Conflict("featured_mapping_missing");
        return await builder.BuildAsync(detail.Snapshot, mapping, cancellationToken);
    }

    private async Task<Result<IntroductionTemplatePayload>> PreparePayloadAsync(IntroductionLayout layout,
        Guid emailTemplateVersionId, IReadOnlyList<IntroductionSelection> selections, CancellationToken cancellationToken)
    {
        var mapping = await mappings.ReadActiveAsync(cancellationToken);
        if (mapping is null) return Result<IntroductionTemplatePayload>.Conflict("featured_mapping_missing");
        try
        {
            var payload = new IntroductionTemplatePayload(mapping, layout, emailTemplateVersionId, selections);
            payload.ToSnapshot();
            return Result.Success(payload);
        }
        catch (IntroductionValidationException exception)
        {
            return Result<IntroductionTemplatePayload>.Invalid(new ValidationError("selections", exception.Message));
        }
    }

    private static Result<IntroductionTemplateDetail> PayloadFailure(Result<IntroductionTemplatePayload> payload)
        => payload.Status == ResultStatus.Invalid
            ? Result<IntroductionTemplateDetail>.Invalid(payload.ValidationErrors)
            : Result<IntroductionTemplateDetail>.Conflict(payload.Errors.FirstOrDefault() ?? "featured_mapping_missing");
}
