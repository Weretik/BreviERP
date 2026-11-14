using Accounting.Application.Abstractions;
using Accounting.Domain.Transactions;
using Microsoft.Extensions.Logging;
using Accounting.Application.Features.Attachments.Shared;

namespace Accounting.Application.Features.Attachments.Queries.GetFinanceAttachmentContent;

public sealed class GetFinanceAttachmentContentHandler(IAccountingDbContext db, IAccountingFileStorage storage,
    ILogger<GetFinanceAttachmentContentHandler> logger)
    : IQueryHandler<GetFinanceAttachmentContentQuery, Result<FinanceAttachmentContent>>
{
    public async ValueTask<Result<FinanceAttachmentContent>> Handle(GetFinanceAttachmentContentQuery query,
        CancellationToken cancellationToken)
    {
        var attachment = await db.Attachments.AsNoTracking().FirstOrDefaultAsync(
            x => x.Id == Domain.Attachments.FinanceAttachmentId.From(query.Id) && x.DeletedAt == null, cancellationToken);
        if (attachment is null) return Result.NotFound(AttachmentMessages.NotFound);
        if (!storage.IsConfigured) return Result.Unavailable(AttachmentMessages.StorageNotConfigured);
        Stream? content;
        try
        {
            content = await storage.OpenReadAsync(attachment.StorageKey, cancellationToken);
        }
        catch (AccountingFileStorageUnavailableException error)
        {
            logger.LogError(error, "Finance file {Key} was not read", attachment.StorageKey);
            return Result.Unavailable(AttachmentMessages.StorageUnavailable);
        }
        return content is null
            ? Result.NotFound(AttachmentMessages.Missing)
            : Result.Success(new FinanceAttachmentContent(content, attachment.FileName, attachment.ContentType));
    }
}
