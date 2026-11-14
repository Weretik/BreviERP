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
    public const string AlreadyLinked = "Цю оплату вже прив'язано до рахунку.";

    public async ValueTask<Result<PurchaseInvoicePaymentsResponse>> Handle(LinkPurchasePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var operation = (await reader.FindAsync([command.TransactionId], cancellationToken)).FirstOrDefault();
        if (operation is not { Expense: true, Active: true })
            return Result<PurchaseInvoicePaymentsResponse>.Invalid(new ValidationError(NotExpense));

        await using var scope = await db.TryBeginWriteAsync(cancellationToken);
        if (scope is null) return Result<PurchaseInvoicePaymentsResponse>.Conflict(PurchaseRegisterSync.AlreadyRunning);
        var message = await db.Messages.FirstOrDefaultAsync(x => x.Id == command.MessageId, cancellationToken);
        if (message is null) return Result<PurchaseInvoicePaymentsResponse>.NotFound(ReviewPurchaseInvoiceHandler.NotFound);
        if (message.ReviewVersion != command.Version)
            return Result<PurchaseInvoicePaymentsResponse>.Conflict(ReviewPurchaseInvoiceHandler.Stale);
        if (await db.Payments.AnyAsync(x => x.MessageId == message.Id && x.TransactionId == command.TransactionId
                                            && x.UnlinkedAt == null, cancellationToken))
            return Result<PurchaseInvoicePaymentsResponse>.Conflict(AlreadyLinked);

        db.Payments.Add(message.LinkPayment(command.TransactionId, command.By, clock.GetUtcNow()));
        await db.SaveChangesAsync(cancellationToken);
        await scope.CommitAsync(cancellationToken);
        return Result.Success(await PurchasePayments.RespondAsync(db, reader, message, cancellationToken));
    }
}
