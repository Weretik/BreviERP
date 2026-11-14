using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.SiteSettings.Shared;
using Content.Domain.SiteSettings;

namespace Content.Application.SiteSettings.Save;

/// <summary>Replaces the settings as a whole with a revision check (CNT-006 SC-02); an empty field removes a code.</summary>
public sealed record SaveSiteSeoSettingsCommand(
    int Revision,
    string? GoogleAnalyticsId,
    string? GoogleTagManagerId,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    int? DefaultOgPhotoId,
    string UpdatedBy) : ICommand<Result<SiteSeoSettingsResponse>>;

public sealed class SaveSiteSeoSettingsCommandValidator : AbstractValidator<SaveSiteSeoSettingsCommand>
{
    public SaveSiteSeoSettingsCommandValidator()
    {
        RuleFor(x => x.Revision).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GoogleAnalyticsId).Must(value => SiteSeoSettings.Clean(value) is not { } id || SiteSeoSettings.IsAnalyticsId(id))
            .WithMessage(SiteSettingsMessages.AnalyticsId);
        RuleFor(x => x.GoogleTagManagerId).Must(value => SiteSeoSettings.Clean(value) is not { } id || SiteSeoSettings.IsTagManagerId(id))
            .WithMessage(SiteSettingsMessages.TagManagerId);
        RuleFor(x => x.GoogleSiteVerification).Must(Verification).WithMessage(SiteSettingsMessages.Verification);
        RuleFor(x => x.BingSiteVerification).Must(Verification).WithMessage(SiteSettingsMessages.Verification);
        RuleFor(x => x.DefaultOgPhotoId).GreaterThan(0).When(x => x.DefaultOgPhotoId is not null);
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }

    private static bool Verification(string? value) =>
        SiteSeoSettings.Clean(value) is not { } code || SiteSeoSettings.IsVerification(code);
}

public sealed class SaveSiteSeoSettingsCommandHandler(IContentDbContext db, IContentMediaReader media, TimeProvider clock)
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
