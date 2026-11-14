using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;
using Accounting.Domain.Transactions;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.Attachments;

/// <summary>The files of one operation, oldest first; removed ones are left out.</summary>
public sealed record GetFinanceAttachmentsQuery(Guid TransactionId) : IQuery<Result<IReadOnlyList<FinanceAttachmentResponse>>>;

public sealed class GetFinanceAttachmentsQueryHandler(IAccountingDbContext db)
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

/// <summary>The content of one file, for «Відкрити».</summary>
public sealed record GetFinanceAttachmentContentQuery(Guid Id) : IQuery<Result<FinanceAttachmentContent>>;

public sealed class GetFinanceAttachmentContentQueryHandler(IAccountingDbContext db, IAccountingFileStorage storage,
    ILogger<GetFinanceAttachmentContentQueryHandler> logger)
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

public sealed class GetFinanceAttachmentsQueryValidator : AbstractValidator<GetFinanceAttachmentsQuery>
{
    public GetFinanceAttachmentsQueryValidator() => RuleFor(x => x.TransactionId).NotEmpty();
}

public sealed class GetFinanceAttachmentContentQueryValidator : AbstractValidator<GetFinanceAttachmentContentQuery>
{
    public GetFinanceAttachmentContentQueryValidator() => RuleFor(x => x.Id).NotEmpty();
}
