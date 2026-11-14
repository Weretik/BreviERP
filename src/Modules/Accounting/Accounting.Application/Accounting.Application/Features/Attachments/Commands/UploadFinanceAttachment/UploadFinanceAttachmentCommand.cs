using System.Security.Cryptography;
using Accounting.Application.Abstractions;
using Accounting.Domain.Attachments;
using Accounting.Domain.Audit;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Microsoft.Extensions.Logging;
using Accounting.Application;
using Accounting.Application.Features.Attachments.Shared;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Journal.Shared;

namespace Accounting.Application.Features.Attachments.Commands.UploadFinanceAttachment;

/// <summary>
/// «Додати файл» to an income or expense: a PDF, JPEG or PNG up to 10 MiB. The file goes to the private storage first;
/// if the row then fails to save, the stored file is removed.
/// </summary>
public sealed record UploadFinanceAttachmentCommand(Guid TransactionId, Stream Content, string FileName, string By)
    : ICommand<Result<FinanceAttachmentResponse>>
{
    /// <summary>The file name without any folder path a browser may send.</summary>
    public string DisplayName => FileName is null ? string.Empty : FileName[(FileName.LastIndexOfAny(['/', '\\']) + 1)..].Trim();
}
