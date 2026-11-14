using Content.Application.Abstractions;
using Content.Domain.SiteSettings;
using Content.Application.Features.SiteSettings.Shared;

namespace Content.Application.Features.SiteSettings.Commands.SaveSiteSeoSettings;

public sealed class SaveSiteSeoSettingsHandler(IContentDbContext db, IContentMediaReader media, TimeProvider clock)
    : ICommandHandler<SaveSiteSeoSettingsCommand, Result<SiteSeoSettingsResponse>>
{
    public async ValueTask<Result<SiteSeoSettingsResponse>> Handle(SaveSiteSeoSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var settings = await db.SiteSeoSettings.FirstOrDefaultAsync(cancellationToken);
        if ((settings?.Revision ?? 0) != command.Revision) return Result.Conflict(SiteSettingsMessages.Stale);

        var picture = await SiteSettingsReads.PictureAsync(media, command.DefaultOgPhotoId, cancellationToken);
        if (command.DefaultOgPhotoId is not null && picture is null) return Result.Conflict(SiteSettingsMessages.PhotoNotFound);

        var now = clock.GetUtcNow();
        if (settings is null)
        {
            settings = new SiteSeoSettings(command.UpdatedBy, now);
            db.SiteSeoSettings.Add(settings);
        }

        var replaced = settings.Replace(command.GoogleAnalyticsId, command.GoogleTagManagerId, command.GoogleSiteVerification,
            command.BingSiteVerification, command.DefaultOgPhotoId, command.UpdatedBy, now);
        if (replaced.IsFailure) return replaced.Error!.ToResult<SiteSeoSettingsResponse>();
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Two first saves at once, or a revision that moved under us.
            return Result.Conflict(SiteSettingsMessages.Stale);
        }

        return SiteSettingsReads.Response(settings, picture);
    }
}
