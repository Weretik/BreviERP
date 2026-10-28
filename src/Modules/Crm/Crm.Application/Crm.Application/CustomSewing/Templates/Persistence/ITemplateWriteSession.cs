using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Domain.CustomSewing.Templates;

namespace Crm.Application.CustomSewing.Templates.Persistence;

public interface ITemplateWriteSession
{
    Task<CustomTemplate?> FindAsync(Guid id, CrmActor actor, CancellationToken ct);
    void Add(CustomTemplate template);
    void Add(CustomTemplateVersion version);
    Task FlushAsync(CancellationToken ct);
    Task<TemplateDetail> DetailAsync(CustomTemplate template, CrmActor actor, CancellationToken ct);
}
