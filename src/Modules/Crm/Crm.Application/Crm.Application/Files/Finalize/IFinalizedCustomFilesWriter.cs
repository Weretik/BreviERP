using Crm.Application.Customers.Access;
using Crm.Application.Files.Contracts;
using Crm.Domain.CustomSewing.Drafts;

namespace Crm.Application.Files.Finalize;

public interface IFinalizedCustomFilesWriter
{
    Task ReplaceAsync(CustomSewingDraft draft, IReadOnlyList<CustomFileSelection> files, CrmActor actor,
        CancellationToken cancellationToken);
}
