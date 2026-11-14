using Accounting.Application.Abstractions;
using Accounting.Domain.Transactions;
using Microsoft.Extensions.Logging;
using Accounting.Application;
using Accounting.Application.Features.Attachments.Shared;

namespace Accounting.Application.Features.Attachments.Queries.GetFinanceAttachments;

public sealed class GetFinanceAttachmentsHandler(IAccountingDbContext db)
    : IQueryHandler<GetFinanceAttachmentsQuery, Result<IReadOnlyList<FinanceAttachmentResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<FinanceAttachmentResponse>>> Handle(GetFinanceAttachmentsQuery query,
        CancellationToken cancellationToken)
    {
        var transactionId = FinanceTransactionId.From(query.TransactionId);
        if (!await db.Transactions.AnyAsync(x => x.Id == transactionId, cancellationToken))
            return Result.NotFound(AccountingMessages.TransactionNotFound);
        var rows = await db.Attachments.AsNoTracking()
            .Where(x => x.TransactionId == transactionId && x.DeletedAt == null)
            .ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<FinanceAttachmentResponse>>(rows
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id.Value)
            .Select(FinanceAttachmentResponse.From).ToList());
    }
}
