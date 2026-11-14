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
    public const string NotLinked = "Цю оплату вже відв'язано. Відкрийте рахунок знову.";

    public async ValueTask<Result<PurchaseInvoicePaymentsResponse>> Handle(UnlinkPurchasePaymentCommand command,
        CancellationToken cancellationToken)
    {
        await using var scope = await db.TryBeginWriteAsync(cancellationToken);
        if (scope is null) return Result<PurchaseInvoicePaymentsResponse>.Conflict(PurchaseRegisterSync.AlreadyRunning);
        var message = await db.Messages.FirstOrDefaultAsync(x => x.Id == command.MessageId, cancellationToken);
        if (message is null) return Result<PurchaseInvoicePaymentsResponse>.NotFound(ReviewPurchaseInvoiceHandler.NotFound);
        if (message.ReviewVersion != command.Version)
            return Result<PurchaseInvoicePaymentsResponse>.Conflict(ReviewPurchaseInvoiceHandler.Stale);
        var link = await db.Payments.FirstOrDefaultAsync(x => x.MessageId == message.Id
            && x.TransactionId == command.TransactionId && x.UnlinkedAt == null, cancellationToken);
        if (link is null) return Result<PurchaseInvoicePaymentsResponse>.Conflict(NotLinked);

        var unlinked = message.UnlinkPayment(link, command.By, clock.GetUtcNow());
        if (unlinked.IsFailure) return unlinked.ToResult<PurchaseInvoicePaymentsResponse>();
        await db.SaveChangesAsync(cancellationToken);
        await scope.CommitAsync(cancellationToken);
        return Result.Success(await PurchasePayments.RespondAsync(db, reader, message, cancellationToken));
    }
}
