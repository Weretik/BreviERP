using Ardalis.Result;
using Crm.Application.Customers.Access;

namespace Crm.Application.Documents.EmailTemplates;
public interface IEmailTemplateStore : IEmailTemplateVersionReader
{
    Task<EmailTemplateSettings> ReadActiveAsync(CancellationToken cancellationToken);
    Task<EmailTemplateDetail?> ReadAsync(Guid versionId, CancellationToken cancellationToken);
    Task<PagedResult<IReadOnlyList<EmailTemplateSummary>>> ListAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<Result<EmailTemplateDetail>> CreateAsync(CrmActor actor, Guid requestId, string name, string subject, string body, CancellationToken cancellationToken);
    Task<Result<EmailTemplateSettings>> ActivateAsync(CrmActor actor, Guid requestId, long expectedRevision, Guid versionId, CancellationToken cancellationToken);
}
