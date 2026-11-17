using Purchasing.Domain.Messages.Identifiers;
using Purchasing.Application.Abstractions;
using Purchasing.Application.Features.Register.Commands.ReviewPurchaseInvoice;
using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;
using Purchasing.Application.Features.Register.Shared;
using BuildingBlocks.Application;

namespace Purchasing.Application.Features.Register.Commands.UnlinkPurchasePayment;

public sealed class UnlinkPurchasePaymentHandler(
    IPurchasingDbContext db,
    IPurchasingPaymentReader reader,
    TimeProvider clock)
    : ICommandHandler<UnlinkPurchasePaymentCommand, Result<PurchaseInvoicePaymentsResponse>>
{
    public async ValueTask<Result<PurchaseInvoicePaymentsResponse>> Handle(UnlinkPurchasePaymentCommand command,
        CancellationToken cancellationToken)
    {
        await using var scope = await db.TryBeginWriteAsync(cancellationToken);
        if (scope is null) return Result<PurchaseInvoicePaymentsResponse>.Conflict(PurchaseRegisterSync.AlreadyRunning);
        var message = await db.Messages.Include(x => x.Payments.Where(payment => payment.UnlinkedAt == null))
            .FirstOrDefaultAsync(x => x.Id == new PurchaseMessageId(command.MessageId), cancellationToken);
        if (message is null) return Result<PurchaseInvoicePaymentsResponse>.NotFound(ReviewPurchaseInvoiceHandler.NotFound);
        if (message.ReviewVersion != command.Version)
            return Result<PurchaseInvoicePaymentsResponse>.Conflict(ReviewPurchaseInvoiceHandler.Stale);
        var unlinked = message.UnlinkPayment(command.TransactionId, command.By, clock.GetUtcNow());
        if (unlinked.IsFailure) return unlinked.ToResult<PurchaseInvoicePaymentsResponse>();
        await db.SaveChangesAsync(cancellationToken);
        await scope.CommitAsync(cancellationToken);
        return Result.Success(await PurchasePayments.RespondAsync(db, reader, message, cancellationToken));
    }
}
