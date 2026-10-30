using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Domain.Quotes.Introductions;

namespace Crm.Application.Quotes.Introductions;

public interface IIntroductionTemplateStore
{
    Task<Result<IntroductionTemplateDetail>> CreateAsync(CrmActor actor, Guid requestId, string name,
        IntroductionTemplatePayload payload, CancellationToken cancellationToken);
    Task<Result<IntroductionTemplateDetail>> ReviseAsync(CrmActor actor, Guid requestId, Guid templatePublicId,
        long expectedRevision, string name, IntroductionTemplatePayload payload, CancellationToken cancellationToken);
    Task<Result<IntroductionTemplateDetail>> SetArchivedAsync(CrmActor actor, Guid requestId, Guid templatePublicId,
        long expectedRevision, bool archived, CancellationToken cancellationToken);
    Task<IntroductionTemplateDetail?> ReadAsync(CrmActor actor, Guid templatePublicId,
        Guid? versionPublicId, CancellationToken cancellationToken);
    Task<PagedResult<IReadOnlyList<IntroductionTemplateSummary>>> ListAsync(CrmActor actor,
        bool includeArchived, int page, int pageSize,
        CancellationToken cancellationToken);
}
