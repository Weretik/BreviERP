using BuildingBlocks.Domain.Entity;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Drafts;
using Crm.Domain.CustomSewing.Identifiers;

namespace Crm.Domain.CustomSewing.Attachments;

public sealed class CustomPrivateFile : BaseEntity<CustomFileId>
{
    public const int MaximumBytes = 20 * 1024 * 1024;
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public CustomDraftId DraftId { get; private set; }
    public CustomSewingDraft Draft { get; private set; } = null!;
    public CrmUserId ActorId { get; private set; }
    public Guid RequestId { get; private set; }
    public string IntentHash { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long Length { get; private set; }
    public string Sha256 { get; private set; } = null!;
    public CustomFileState State { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? UploadedAt { get; private set; }
    private CustomPrivateFile() { }

    public static CustomPrivateFile Reserve(CustomDraftId draft, CrmUserId actor, Guid requestId,
        string hash, string contentType, long length, string sha256, DateTimeOffset at, DateTimeOffset expiresAt)
    {
        if (draft.Value <= 0 || actor.Value <= 0 || requestId == Guid.Empty || length is <= 0 or > MaximumBytes
            || hash is null || sha256 is null || hash.Length != 64 || sha256.Length != 64 || expiresAt <= at
            || hash.Any(x => !char.IsAsciiHexDigit(x)) || sha256.Any(x => !char.IsAsciiHexDigit(x))
            || contentType is not ("application/pdf" or "image/png" or "image/jpeg"))
            throw new CustomSewingValidationException("invalid_private_file_reservation");
        return new() { DraftId = draft, ActorId = actor, RequestId = requestId, IntentHash = hash.ToUpperInvariant(),
            ContentType = contentType, Length = length, Sha256 = sha256.ToUpperInvariant(), CreatedAt = at, ExpiresAt = expiresAt };
    }

    public void Complete(DateTimeOffset at)
    {
        if (State != CustomFileState.Reserved || at >= ExpiresAt)
            throw new CustomSewingValidationException("private_file_upload_expired_or_changed");
        State = CustomFileState.Uploaded;
        UploadedAt = at;
    }

    public void BeginCleanup(DateTimeOffset at)
    {
        if (ExpiresAt > at || State is not (CustomFileState.Reserved or CustomFileState.Uploaded or CustomFileState.Deleting))
            throw new CustomSewingValidationException("private_file_not_expired");
        State = CustomFileState.Deleting;
    }

    public void FinishCleanup()
    {
        if (State != CustomFileState.Deleting) throw new CustomSewingValidationException("private_file_not_deleting");
        State = CustomFileState.Expired;
    }
}
