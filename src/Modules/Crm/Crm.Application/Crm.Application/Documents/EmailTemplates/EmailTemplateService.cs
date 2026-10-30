using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Documents.EmailTemplates;

namespace Crm.Application.Documents.EmailTemplates;
public sealed class EmailTemplateService(IEmailTemplateStore store)
{
    public Task<EmailTemplateSettings> ActiveAsync(CancellationToken ct) => store.ReadActiveAsync(ct);
    public Task<EmailTemplateDetail?> ReadAsync(Guid id, CancellationToken ct) => store.ReadAsync(id, ct);
    public Task<PagedResult<IReadOnlyList<EmailTemplateSummary>>> ListAsync(int page, int pageSize, CancellationToken ct)
        => store.ListAsync(page, pageSize, ct);
    public Task<Result<EmailTemplateDetail>> CreateAsync(CrmActor actor, Guid requestId, string name, string subject, string body, CancellationToken ct)
    {
        if (!actor.IsAdmin || actor.UserId <= 0) return Task.FromResult(Result<EmailTemplateDetail>.Forbidden());
        if (requestId == Guid.Empty) return Task.FromResult(Result<EmailTemplateDetail>.Invalid(new ValidationError("requestId", "invalid_request")));
        try { EmailTemplateVersion.Create(1, name, subject, body, new CrmUserId(actor.UserId)); }
        catch (EmailTemplateValidationException e) { return Task.FromResult(Result<EmailTemplateDetail>.Invalid(new ValidationError("template", e.Message))); }
        return store.CreateAsync(actor, requestId, name, subject, body, ct);
    }
    public Task<Result<EmailTemplateSettings>> ActivateAsync(CrmActor actor, Guid requestId, long expectedRevision, Guid versionId, CancellationToken ct)
    {
        if (!actor.IsAdmin || actor.UserId <= 0) return Task.FromResult(Result<EmailTemplateSettings>.Forbidden());
        if (requestId == Guid.Empty || expectedRevision <= 0 || versionId == Guid.Empty)
            return Task.FromResult(Result<EmailTemplateSettings>.Invalid(new ValidationError("request", "invalid_request")));
        return store.ActivateAsync(actor, requestId, expectedRevision, versionId, ct);
    }
}
