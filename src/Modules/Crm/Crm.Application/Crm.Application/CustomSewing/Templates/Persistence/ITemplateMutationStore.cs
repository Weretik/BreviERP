using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Templates.Contracts;

namespace Crm.Application.CustomSewing.Templates.Persistence;

public interface ITemplateMutationStore
{
    Task<Result<TemplateDetail>> ExecuteAsync(string operation, Guid requestId, CrmActor actor, object intent,
        Func<ITemplateWriteSession, CancellationToken, Task<Result<TemplateDetail>>> work, CancellationToken ct);
}
