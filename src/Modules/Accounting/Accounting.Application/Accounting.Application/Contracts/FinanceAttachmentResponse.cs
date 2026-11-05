using Accounting.Domain.Attachments;

namespace Accounting.Application.Contracts;

/// <summary>A file of an operation; its content is <c>GET /api/accounting/attachments/{id}/content</c>.</summary>
public sealed record FinanceAttachmentResponse(
    Guid Id,
    Guid TransactionId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string CreatedBy,
    DateTimeOffset CreatedAt)
{
    public static FinanceAttachmentResponse From(FinanceAttachment attachment) => new(attachment.Id.Value,
        attachment.TransactionId.Value, attachment.FileName, attachment.ContentType, attachment.SizeBytes,
        attachment.CreatedBy, attachment.CreatedAt);
}

/// <summary>The content of a file, read for an inline view.</summary>
public sealed record FinanceAttachmentContent(Stream Content, string FileName, string ContentType);
