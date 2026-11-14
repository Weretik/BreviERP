using Accounting.Domain.Attachments;

namespace Accounting.Application.Features.Attachments.Queries.GetFinanceAttachmentContent;

/// <summary>The content of a file, read for an inline view.</summary>
public sealed record FinanceAttachmentContent(Stream Content, string FileName, string ContentType);
