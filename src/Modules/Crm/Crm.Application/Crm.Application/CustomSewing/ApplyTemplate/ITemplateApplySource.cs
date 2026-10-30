using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Templates.Contracts;
namespace Crm.Application.CustomSewing.ApplyTemplate;

public interface ITemplateApplySource
{
    // Called inside the custom-draft transaction; serializes with template revision/archive.
    Task<TemplateDetail?> LockAsync(Guid templateId, Guid versionId, CrmActor actor, CancellationToken ct);
}
