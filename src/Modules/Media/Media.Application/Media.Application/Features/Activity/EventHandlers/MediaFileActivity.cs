using ActivityLog.Contracts.Activity;
using Media.Domain.Files;

namespace Media.Application.Features.Activity.EventHandlers;

/// <summary>How photos read in «Журнал дій» (ACT-001): uploaded, renamed or given a new alt text, moved, deleted.</summary>
public sealed class MediaFileActivity : IActivityDescriber
{
    public ActivityDescription? Describe(object domainEvent)
    {
        if (domainEvent is not MediaFileRecorded e) return null;
        var file = e.File;
        var changes = new ActivityChanges();
        if (e.Action == MediaFileAction.Changed || e.NameBefore is not null)
            changes.Add("Назва", e.NameBefore, file.FileName).Add("Alt-текст", e.AltBefore, file.Alt);
        if (e.Action == MediaFileAction.Changed && !changes.Any) return null;
        var (action, summary) = e.Action switch
        {
            MediaFileAction.Uploaded => (ActivityActions.Created, (string?)null),
            MediaFileAction.Deleted => (ActivityActions.Deleted, null),
            MediaFileAction.Moved => (ActivityActions.Changed, "Перенесено в іншу папку"),
            _ => (ActivityActions.Changed, null),
        };
        return new ActivityDescription(action, "media-file", "Фото", file.Id.ToString(), file.FileName, summary,
            changes.Items, "Media");
    }
}
