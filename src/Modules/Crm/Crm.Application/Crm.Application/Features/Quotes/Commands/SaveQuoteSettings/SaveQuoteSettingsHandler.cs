using Crm.Domain.Quotes.Settings;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Features.Quotes.Shared.Settings;
using Crm.Application.Features.Quotes.Shared.Settings.Reading;
using Crm.Application.Abstractions.Quotes;
using BuildingBlocks.Application;

namespace Crm.Application.Features.Quotes.Commands.SaveQuoteSettings;

/// <summary>Saves the whole settings page (R-13); only Director and Admin, 409 when someone saved first.</summary>
public sealed class SaveQuoteSettingsHandler(IQuoteStore store, IQuoteStaffReader staff, TimeProvider clock)
    : ICommandHandler<SaveQuoteSettingsCommand, Result<QuoteSettingsResponse>>
{
    public async ValueTask<Result<QuoteSettingsResponse>> Handle(SaveQuoteSettingsCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanSeeAll) return Result.Forbidden(QuoteSettingsMessages.Forbidden);
        var actor = await staff.PersonAsync(command.Actor, cancellationToken);
        if (actor is null) return Result.Forbidden(QuoteMessages.ActorUnknown);

        var members = await staff.ListAsync(cancellationToken);
        var known = members.Select(x => x.UserId).ToHashSet();
        if (command.Input.StaffContacts.Any(x => !known.Contains(x.UserId)))
            return Result.Invalid(new ValidationError(QuoteMessages.StaffNotFound));

        var saved = await store.FindSettingsAsync(tracking: true, cancellationToken);
        if ((saved?.Version ?? 0) != command.Input.Version) return Result.Conflict(QuoteSettingsMessages.Stale);

        var content = command.Input.ToContent();
        var now = clock.GetUtcNow();
        if (saved is null)
        {
            var created = QuoteSettings.Create(content, actor.Name, now);
            if (created.IsFailure) return created.Error!.ToResult<QuoteSettingsResponse>();
            saved = created.Value;
            store.QuoteSettings.Add(saved);
        }
        else
        {
            var applied = saved.Apply(content, actor.Name, now);
            if (applied.IsFailure) return applied.Error!.ToResult<QuoteSettingsResponse>();
        }

        try
        {
            await store.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(QuoteSettingsMessages.Stale);
        }
        catch (DbUpdateException exception) when (store.IsDuplicateSettings(exception))
        {
            return Result.Conflict(QuoteSettingsMessages.Stale);
        }

        return Result.Success(saved.ToResponse(members, await store.MailboxEmailsAsync(cancellationToken)));
    }
}
