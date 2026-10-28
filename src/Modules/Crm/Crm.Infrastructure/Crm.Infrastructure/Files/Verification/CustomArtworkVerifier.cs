using Ardalis.Result;
using Crm.Application.Abstractions.Files;
using Crm.Application.Customers.Access;
using Crm.Application.Files.Verification;
using Crm.Infrastructure.Persistence;

namespace Crm.Infrastructure.Files.Verification;

public sealed class CustomArtworkVerifier(CrmDbContext db, IPrivateFileStore bytes, TimeProvider? clock = null)
    : ICustomArtworkVerifier
{
    public Task<Result<VerifiedCustomArtwork>> VerifyAsync(Guid draftId, IReadOnlyCollection<Guid> artworkFileIds,
        CrmActor actor, CancellationToken ct)
        => new CustomFileAvailability(db, bytes, clock ?? TimeProvider.System).VerifyAsync(draftId, artworkFileIds, actor, true, ct);
}
