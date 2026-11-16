using ActivityLog.Contracts.Activity;
using Media.Domain.Folders;

namespace Media.Application.Features.Activity.EventHandlers;

/// <summary>How photo folders read in «Журнал дій» (ACT-001): created, renamed, moved, deleted.</summary>
public sealed class MediaFolderActivity : IActivityDescriber
{
    public ActivityDescription? Describe(object domainEvent)
    {
        if (domainEvent is not MediaFolderRecorded e) return null;
        var folder = e.Folder;
        var changes = new ActivityChanges();
        if (e.Action == MediaFolderAction.Renamed) changes.Add("Назва", e.NameBefore, folder.Name);
        var (action, summary) = e.Action switch
        {
            MediaFolderAction.Created => (ActivityActions.Created, (string?)null),
            MediaFolderAction.Deleted => (ActivityActions.Deleted, null),
            MediaFolderAction.Moved => (ActivityActions.Changed, "Перенесено в іншу папку"),
            _ => (ActivityActions.Changed, null),
        };
        return new ActivityDescription(action, "media-folder", "Папка фото", folder.Id.ToString(), folder.Name, summary,
            changes.Items, "Media");
    }
}
