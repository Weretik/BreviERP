using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Journal;
using Accounting.Domain.Attachments;
using Accounting.Domain.Audit;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.Attachments;

/// <summary>
/// «Видалити» a file of an operation: the row is marked as removed (so «Історія» still names it) and the stored file
/// is deleted.
/// </summary>
public sealed record DeleteFinanceAttachmentCommand(Guid Id, string By) : ICommand<Result>;

public sealed class DeleteFinanceAttachmentCommandValidator : AbstractValidator<DeleteFinanceAttachmentCommand>
{
    public DeleteFinanceAttachmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class DeleteFinanceAttachmentCommandHandler(IAccountingDbContext db, IAccountingFileStorage storage,
    TimeProvider clock, ILogger<DeleteFinanceAttachmentCommandHandler> logger)
    : ICommandHandler<DeleteFinanceAttachmentCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteFinanceAttachmentCommand command, CancellationToken cancellationToken)
    {
        var attachment = await db.Attachments.FirstOrDefaultAsync(
            x => x.Id == FinanceAttachmentId.From(command.Id) && x.DeletedAt == null, cancellationToken);
        if (attachment is null) return Result.NotFound(AttachmentMessages.NotFound);

        var now = clock.GetUtcNow();
        var before = AuditTrail.Snapshot(attachment);
        attachment.Delete(command.By, now);
        AuditTrail.Add(db, AuditEntity.Transaction, attachment.TransactionId.Value, AuditAction.AttachmentDelete,
            command.By, now, before, null);
        await db.SaveChangesAsync(cancellationToken);

        if (storage.IsConfigured)
        {
            try
            {
                await storage.DeleteAsync(attachment.StorageKey, CancellationToken.None);
            }
            catch (Exception error)
            {
                // The file is already gone from the operation; a leftover in the private storage is harmless.
                logger.LogWarning(error, "Finance file {Key} was not deleted from the storage", attachment.StorageKey);
            }
        }
        return Result.Success();
    }
}
