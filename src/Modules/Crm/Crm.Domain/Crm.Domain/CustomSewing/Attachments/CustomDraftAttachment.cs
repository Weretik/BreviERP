using Crm.Domain.CustomSewing.Identifiers;

namespace Crm.Domain.CustomSewing.Attachments;

public sealed class CustomDraftAttachment
{
    public CustomDraftId DraftId { get; private set; }
    public CustomFileId FileId { get; private set; }
    public CustomPrivateFile File { get; private set; } = null!;
    public bool ShowInQuote { get; private set; }
    public bool Production { get; private set; }
    private CustomDraftAttachment() { }

    public static CustomDraftAttachment Create(CustomDraftId draftId, CustomFileId fileId, bool showInQuote, bool production)
    {
        if (draftId.Value <= 0 || fileId.Value <= 0) throw new CustomSewingValidationException("invalid_private_file_link");
        return new() { DraftId = draftId, FileId = fileId, ShowInQuote = showInQuote, Production = production };
    }
}
