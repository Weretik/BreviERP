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

public sealed class UploadFinanceAttachmentHandler(IAccountingDbContext db, IAccountingFileStorage storage,
    TimeProvider clock, ILogger<UploadFinanceAttachmentHandler> logger)
    : ICommandHandler<UploadFinanceAttachmentCommand, Result<FinanceAttachmentResponse>>
{
    public async ValueTask<Result<FinanceAttachmentResponse>> Handle(UploadFinanceAttachmentCommand command,
        CancellationToken cancellationToken)
    {
        if (!storage.IsConfigured) return Result.Unavailable(AttachmentMessages.StorageNotConfigured);
        var transactionId = FinanceTransactionId.From(command.TransactionId);
        var transaction = await db.Transactions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == transactionId, cancellationToken);
        if (transaction is null) return Result.NotFound(AccountingMessages.TransactionNotFound);
        if (transaction.Status != MovementStatus.Active)
            return Result.Invalid(new ValidationError("File", AttachmentMessages.TransactionVoided));

        // At most 10 MiB is read; one byte more means the file is too large.
        using var buffer = new MemoryStream();
        await CopyAtMostAsync(command.Content, buffer, FinanceAttachment.MaxBytes + 1, cancellationToken);
        if (buffer.Length == 0) return Result.Invalid(new ValidationError("File", AttachmentMessages.Empty));
        if (buffer.Length > FinanceAttachment.MaxBytes)
            return Result.Invalid(new ValidationError("File", AttachmentMessages.TooLarge));
        var bytes = buffer.GetBuffer().AsMemory(0, (int)buffer.Length);
        if (FinanceAttachmentKind.Detect(bytes.Span[..Math.Min(bytes.Length, FinanceAttachmentKind.SignatureLength)])
            is not { } kind)
            return Result.Invalid(new ValidationError("File", AttachmentMessages.WrongType));

        var now = clock.GetUtcNow();
        var created = FinanceAttachment.Create(FinanceAttachmentId.New(), transactionId, command.DisplayName, kind,
            bytes.Length, Convert.ToHexStringLower(SHA256.HashData(bytes.Span)), command.By, now);
        if (created.IsFailure) return created.Error!.ToResult<FinanceAttachmentResponse>();
        var attachment = created.Value;
        buffer.Position = 0;
        try
        {
            await storage.UploadAsync(attachment.StorageKey, buffer, kind.ContentType, cancellationToken);
        }
        catch (AccountingFileStorageUnavailableException error)
        {
            logger.LogError(error, "Finance file {Key} was not stored", attachment.StorageKey);
            return Result.Unavailable(AttachmentMessages.StorageUnavailable);
        }

        db.Attachments.Add(attachment);
        AuditTrail.Add(db, AuditEntity.Transaction, command.TransactionId, AuditAction.AttachmentAdd, command.By, now,
            null, AuditTrail.Snapshot(attachment));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // The storage already has the file; remove it so it does not stay without a row.
            try
            {
                await storage.DeleteAsync(attachment.StorageKey, CancellationToken.None);
            }
            catch (Exception error)
            {
                logger.LogWarning(error, "Finance file {Key} was left in the storage without a row", attachment.StorageKey);
            }
            throw;
        }

        return Result.Success(FinanceAttachmentResponse.From(attachment));
    }

    private static async Task CopyAtMostAsync(Stream source, Stream target, long limit, CancellationToken cancellationToken)
    {
        var chunk = new byte[81920];
        long total = 0;
        int read;
        while (total < limit &&
               (read = await source.ReadAsync(chunk.AsMemory(0, (int)Math.Min(chunk.Length, limit - total)),
                   cancellationToken)) > 0)
        {
            await target.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
            total += read;
        }
    }
}
