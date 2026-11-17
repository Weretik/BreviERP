using BuildingBlocks.Application;
using Purchasing.Application.Abstractions;
using Purchasing.Application.Features.Register.Commands.ReviewPurchaseInvoice;
using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;
using Purchasing.Application.Features.Register.Shared;

namespace Purchasing.Application.Features.Register.Commands.LinkPurchasePayment;

public sealed class LinkPurchasePaymentHandler(
    IPurchasingDbContext db,
    IPurchasingPaymentReader reader,
    TimeProvider clock)
    : ICommandHandler<LinkPurchasePaymentCommand, Result<PurchaseInvoicePaymentsResponse>>
{
    public const string NotExpense = "Оплатою може бути лише чинна витрата з «Бухгалтерії».";

    public async ValueTask<Result<PurchaseInvoicePaymentsResponse>> Handle(LinkPurchasePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var operation = (await reader.FindAsync([command.TransactionId], cancellationToken)).FirstOrDefault();
        if (operation is not { Expense: true, Active: true })
            return Result<PurchaseInvoicePaymentsResponse>.Invalid(new ValidationError(NotExpense));

        await using var scope = await db.TryBeginWriteAsync(cancellationToken);
        if (scope is null) return Result<PurchaseInvoicePaymentsResponse>.Conflict(PurchaseRegisterSync.AlreadyRunning);
        var message = await db.Messages.Include(x => x.Payments.Where(payment => payment.UnlinkedAt == null))
            .FirstOrDefaultAsync(x => x.Id == command.MessageId, cancellationToken);
        if (message is null) return Result<PurchaseInvoicePaymentsResponse>.NotFound(ReviewPurchaseInvoiceHandler.NotFound);
        if (message.ReviewVersion != command.Version)
            return Result<PurchaseInvoicePaymentsResponse>.Conflict(ReviewPurchaseInvoiceHandler.Stale);
        var linked = message.LinkPayment(command.TransactionId, command.By, clock.GetUtcNow());
        if (linked.IsFailure) return linked.ToResult<PurchaseInvoicePaymentsResponse>();
        await db.SaveChangesAsync(cancellationToken);
        await scope.CommitAsync(cancellationToken);
        return Result.Success(await PurchasePayments.RespondAsync(db, reader, message, cancellationToken));
    }
}
