using Accounting.Application.Abstractions;
using Accounting.Domain.Transactions;
using Microsoft.Extensions.Logging;
using Accounting.Application.Features.Attachments.Shared;

namespace Accounting.Application.Features.Attachments.Queries.GetFinanceAttachments;

/// <summary>The files of one operation, oldest first; removed ones are left out.</summary>
public sealed record GetFinanceAttachmentsQuery(Guid TransactionId) : IQuery<Result<IReadOnlyList<FinanceAttachmentResponse>>>;
