using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Files.Verification;
using Crm.Domain.CustomSewing.Drafts;
using Crm.Domain.CustomSewing.Pricing;

namespace Crm.Application.CustomSewing.Pricing;

public sealed class CustomSewingPriceCalculator(ICustomSewingPricing pricing, ICustomArtworkVerifier? artworkVerifier)
{
    public async Task<Result<CustomPriceBasis>> CalculateAsync(CustomSewingDraft draft, CrmActor actor,
        DateTimeOffset at, CancellationToken cancellationToken)
    {
        var ids = draft.Configuration.Applications.Where(x => x.ArtworkFileId is not null)
            .Select(x => x.ArtworkFileId!.Value).Distinct().ToArray();
        if (ids.Length == 0)
            return Result.Success(await pricing.CalculateAsync(draft.Configuration, at, cancellationToken));
        if (artworkVerifier is null)
            return Result.Invalid(new ValidationError("artworkFileId", "ArtworkFileNotVerified"));
        var verified = await artworkVerifier.VerifyAsync(draft.PublicId, ids, actor, cancellationToken);
        if (!verified.IsSuccess) return verified.Map(_ => (CustomPriceBasis)null!);
        if (verified.Value is not { } proof || proof.DraftPublicId != draft.PublicId || proof.FileIds is null
            || !ids.ToHashSet().SetEquals(proof.FileIds))
            return Result.Invalid(new ValidationError("artworkFileId", "ArtworkFileNotVerified"));
        return Result.Success(await pricing.CalculateAsync(draft.Configuration, at, cancellationToken, artworkVerified: true));
    }
}
