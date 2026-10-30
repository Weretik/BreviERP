using Crm.Application.Customers.Access;
using Crm.Domain.Customers;
using Crm.Domain.CustomSewing.Drafts;
using Crm.Domain.CustomSewing.Identifiers;
using Crm.Domain.CustomSewing.Pricing;
using Crm.Domain.CustomSewing.Versions;
using Crm.Domain.Quotes;

namespace Crm.Application.CustomSewing.Persistence;

public interface ICustomWriteSession
{
    Task<CustomSewingDraft?> FindDraftAsync(Guid publicId, CrmActor actor, CancellationToken cancellationToken);
    Task<Customer?> FindCustomerAsync(Guid publicId, CrmActor actor, CancellationToken cancellationToken);
    Task<Quote?> FindQuoteAsync(Guid publicId, CrmActor actor, CancellationToken cancellationToken);
    void Add(CustomSewingDraft draft);
    void Add(CustomSewingVersion version);
    void AddBasis(CustomSewingDraft draft, int actorId, CustomPriceBasis basis);
    Task<CustomPriceBasis?> FindBasisAsync(Guid token, CustomDraftId draftId, long revision, int actorId, CancellationToken cancellationToken);
    Task FlushAsync(CancellationToken cancellationToken);
    Task PinFilesAsync(CustomSewingVersion version, CrmActor actor, CancellationToken cancellationToken);
    Task<Crm.Application.CustomSewing.Contracts.CustomDetail> DetailAsync(CustomSewingDraft draft, CancellationToken cancellationToken);
}
