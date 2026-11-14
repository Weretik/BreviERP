using Crm.Domain.Embroidery.Settings;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;
using Crm.Application.Abstractions.Embroidery;
using Crm.Application.Abstractions.Quotes;
using BuildingBlocks.Application;

namespace Crm.Application.Features.Embroidery.Commands.SaveEmbroiderySettings;

/// <summary>Saves the whole «Налаштування» page; only Director and Admin, 409 when someone saved first.</summary>
public sealed class SaveEmbroiderySettingsHandler(
    IEmbroideryStore store,
    IEmbroideryStitchRateReader stitchRate,
    IQuoteStaffReader staff,
    TimeProvider clock)
    : ICommandHandler<SaveEmbroiderySettingsCommand, Result<EmbroiderySettingsResponse>>
{
    public async ValueTask<Result<EmbroiderySettingsResponse>> Handle(SaveEmbroiderySettingsCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.SeesCost) return Result.Forbidden(EmbroideryMessages.Forbidden);
        var actor = await staff.FindAsync(command.Actor.UserId, cancellationToken);
        if (actor is null) return Result.Forbidden(EmbroideryMessages.ActorUnknown);

        var saved = await store.FindSettingsAsync(tracking: true, cancellationToken);
        if ((saved?.Version ?? 0) != command.Input.Version) return Result.Conflict(EmbroideryMessages.Stale);

        var content = command.Input.ToContent();
        var now = clock.GetUtcNow();
        if (saved is null)
        {
            var created = EmbroiderySettings.Create(content, actor.Name, now);
            if (created.IsFailure) return created.Error!.ToResult<EmbroiderySettingsResponse>();
            saved = created.Value;
            store.EmbroiderySettings.Add(saved);
        }
        else
        {
            var applied = saved.Apply(content, actor.Name, now);
            if (applied.IsFailure) return applied.Error!.ToResult<EmbroiderySettingsResponse>();
        }

        try
        {
            await store.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(EmbroideryMessages.Stale);
        }
        catch (DbUpdateException exception) when (store.IsDuplicateEmbroiderySettings(exception))
        {
            return Result.Conflict(EmbroideryMessages.Stale);
        }

        return Result.Success(saved.ToResponse(await stitchRate.ReadAsync(cancellationToken)));
    }
}
