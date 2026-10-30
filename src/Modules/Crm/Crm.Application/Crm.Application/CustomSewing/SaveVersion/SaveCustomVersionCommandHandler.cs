using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Mediator;
using Crm.Application.CustomSewing.Persistence;
using Crm.Application.CustomSewing.Pricing;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Versions;
using Crm.Application.Files.Verification;

namespace Crm.Application.CustomSewing.SaveVersion;

public sealed class SaveCustomVersionCommandHandler(ICustomMutationStore store, ICustomSewingPricing pricing,
    TimeProvider? timeProvider = null, ICustomArtworkVerifier? artworkVerifier = null)
    : ICommandHandler<SaveCustomVersionCommand, Result<CustomDetail>>
{
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    public async ValueTask<Result<CustomDetail>> Handle(SaveCustomVersionCommand command, CancellationToken cancellationToken)
    {
        if (command.RequestId == Guid.Empty || command.Actor.UserId <= 0 || command.ExpectedRevision <= 0 || command.DraftId == Guid.Empty)
            return Result.Invalid(new ValidationError("request", "invalid_request"));
        return await store.ExecuteAsync("SaveCustomVersion", command.RequestId, command.Actor, command, async (session, ct) =>
        {
            var draft = await session.FindDraftAsync(command.DraftId, command.Actor, ct);
            if (draft is null) return Result.NotFound();
            if (draft.Revision != command.ExpectedRevision || draft.Quote?.Archived == true)
                return Result.Conflict("revision_conflict");
            var basis = await session.FindBasisAsync(command.BasisToken, draft.Id, draft.Revision, command.Actor.UserId, ct);
            if (basis is null) return Result.Conflict("custom_basis_missing_expired_or_changed");
            if (!basis.IsCalculated) return Result.Invalid(new ValidationError("basisToken", "custom_calculation_blocked"));
            var calculated = await new CustomSewingPriceCalculator(pricing, artworkVerifier)
                .CalculateAsync(draft, command.Actor, _time.GetUtcNow(), ct);
            if (!calculated.IsSuccess) return calculated.Map(_ => (CustomDetail)null!);
            var current = calculated.Value;
            if (!current.IsCalculated || !SameSources(basis, current)) return Result.Conflict("custom_source_changed");
            if (basis.ExpiresAt <= _time.GetUtcNow())
                return Result.Conflict("custom_basis_missing_expired_or_changed");
            var version = CustomSewingVersion.Create(draft.Id, draft.VersionNumber + 1, draft.CurrentVersionId,
                draft.Configuration, basis, new CrmUserId(command.Actor.UserId));
            session.Add(version);
            await session.FlushAsync(ct);
            await session.PinFilesAsync(version, command.Actor, ct);
            draft.SetVersion(command.ExpectedRevision, version);
            await session.FlushAsync(ct);
            return Result.Success(await session.DetailAsync(draft, ct));
        }, cancellationToken);
    }

    private static bool SameSources(Crm.Domain.CustomSewing.Pricing.CustomPriceBasis before,
        Crm.Domain.CustomSewing.Pricing.CustomPriceBasis after) =>
        before.FormulaVersion == after.FormulaVersion && before.SettingsVersionId == after.SettingsVersionId
        && before.References.OrderBy(x => x.Key, StringComparer.Ordinal).SequenceEqual(after.References.OrderBy(x => x.Key, StringComparer.Ordinal))
        && before.Retail == after.Retail && before.Wholesale == after.Wholesale && before.LargeWholesale == after.LargeWholesale
        && before.InternalCost == after.InternalCost && before.Breakdown == after.Breakdown;
}
