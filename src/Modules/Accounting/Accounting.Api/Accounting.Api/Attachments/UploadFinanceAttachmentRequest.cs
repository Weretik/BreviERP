using Microsoft.AspNetCore.Http;

namespace Accounting.Api.Attachments;

/// <param name="File">A PDF, JPEG or PNG up to 10 MiB.</param>
public sealed record UploadFinanceAttachmentRequest(IFormFile File);
