using Accounting.Application.Abstractions;
using Accounting.Domain.Transactions;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.Features.Attachments.Queries.GetFinanceAttachmentContent;

/// <summary>The content of one file, for «Відкрити».</summary>
public sealed record GetFinanceAttachmentContentQuery(Guid Id) : IQuery<Result<FinanceAttachmentContent>>;
