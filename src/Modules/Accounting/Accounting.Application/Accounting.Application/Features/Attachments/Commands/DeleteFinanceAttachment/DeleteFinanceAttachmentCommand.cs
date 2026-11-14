using Accounting.Application.Abstractions;
using Accounting.Domain.Attachments;
using Accounting.Domain.Audit;
using Microsoft.Extensions.Logging;
using Accounting.Application.Features.Attachments.Shared;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Journal.Shared;

namespace Accounting.Application.Features.Attachments.Commands.DeleteFinanceAttachment;

/// <summary>
/// «Видалити» a file of an operation: the row is marked as removed (so «Історія» still names it) and the stored file
/// is deleted.
/// </summary>
public sealed record DeleteFinanceAttachmentCommand(Guid Id, string By) : ICommand<Result>;
