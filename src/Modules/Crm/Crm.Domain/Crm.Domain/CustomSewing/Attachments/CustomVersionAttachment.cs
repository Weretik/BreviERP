using Crm.Domain.CustomSewing.Identifiers;
using Crm.Domain.CustomSewing.Versions;

namespace Crm.Domain.CustomSewing.Attachments;

public sealed class CustomVersionAttachment
{
    public CustomVersionId VersionId { get; private set; }
    public CustomSewingVersion Version { get; private set; } = null!;
    public CustomDraftId DraftId { get; private set; }
    public CustomFileId FileId { get; private set; }
    public CustomPrivateFile File { get; private set; } = null!;
    public bool ShowInQuote { get; private set; }
    public bool Production { get; private set; }
    public string ContentType { get; private set; } = null!;
    public long Length { get; private set; }
    public string Sha256 { get; private set; } = null!;
    private CustomVersionAttachment() { }

    public static CustomVersionAttachment Pin(CustomSewingVersion version, CustomDraftAttachment link)
    {
        if (version.Id.Value <= 0 || version.DraftId != link.DraftId || link.File.State != CustomFileState.Uploaded)
            throw new CustomSewingValidationException("private_file_not_finalized");
        return new() { VersionId = version.Id, DraftId = link.DraftId, FileId = link.FileId,
            ShowInQuote = link.ShowInQuote, Production = link.Production,
            ContentType = link.File.ContentType, Length = link.File.Length, Sha256 = link.File.Sha256 };
    }
}
