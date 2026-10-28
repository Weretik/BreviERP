using Ardalis.Result;
using Crm.Application.Customers.Access;

namespace Crm.Application.Files.Verification;

public interface ICustomArtworkVerifier
{
    Task<Result<VerifiedCustomArtwork>> VerifyAsync(Guid draftId, IReadOnlyCollection<Guid> artworkFileIds,
        CrmActor actor, CancellationToken cancellationToken);
}
