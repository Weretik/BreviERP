namespace Accounting.Domain.Attachments;

[ValueObject<Guid>]
public readonly partial struct FinanceAttachmentId
{
    public static FinanceAttachmentId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) =>
        value == Guid.Empty ? Validation.Invalid("Attachment id must not be empty.") : Validation.Ok;
}
