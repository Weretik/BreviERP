using Purchasing.Application.Abstractions;
using Purchasing.Application.Contracts;
using Purchasing.Application.Register.Review;
using Purchasing.Application.Register.Sync;
using Purchasing.Domain.Messages;

namespace Purchasing.Application.Register.Payments;

/// <summary>
/// «Прив'язати оплату»: links an active expense of «Бухгалтерія» to the invoice. <paramref name="Version"/> is the review
/// version the user saw; the same operation may pay several invoices.
/// </summary>
public sealed record LinkPurchasePaymentCommand(string MessageId, Guid TransactionId, int Version, string By)
    : ICommand<Result<PurchaseInvoicePaymentsResponse>>;

/// <summary>«Відв'язати»: removes an active payment link; the link stays in the history.</summary>
public sealed record UnlinkPurchasePaymentCommand(string MessageId, Guid TransactionId, int Version, string By)
    : ICommand<Result<PurchaseInvoicePaymentsResponse>>;

public sealed class LinkPurchasePaymentCommandValidator : AbstractValidator<LinkPurchasePaymentCommand>
{
    public LinkPurchasePaymentCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.Version).GreaterThanOrEqualTo(0);
        RuleFor(x => x.By).NotEmpty().MaximumLength(PurchaseMessage.ReviewerMaxLength);
    }
}

public sealed class UnlinkPurchasePaymentCommandValidator : AbstractValidator<UnlinkPurchasePaymentCommand>
{
    public UnlinkPurchasePaymentCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.Version).GreaterThanOrEqualTo(0);
        RuleFor(x => x.By).NotEmpty().MaximumLength(PurchaseMessage.ReviewerMaxLength);
    }
}

public sealed class PurchasePaymentCommandsHandler(
    IPurchasingDbContext db,
    IPurchasingPaymentReader reader,
    TimeProvider clock)
    : ICommandHandler<LinkPurchasePaymentCommand, Result<PurchaseInvoicePaymentsResponse>>,
        ICommandHandler<UnlinkPurchasePaymentCommand, Result<PurchaseInvoicePaymentsResponse>>
{
    public const string NotExpense = "Оплатою може бути лише чинна витрата з «Бухгалтерії».";
    public const string AlreadyLinked = "Цю оплату вже прив'язано до рахунку.";
    public const string NotLinked = "Цю оплату вже відв'язано. Відкрийте рахунок знову.";

    public async ValueTask<Result<PurchaseInvoicePaymentsResponse>> Handle(LinkPurchasePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var operation = (await reader.FindAsync([command.TransactionId], cancellationToken)).FirstOrDefault();
        if (operation is not { Expense: true, Active: true })
            return Result<PurchaseInvoicePaymentsResponse>.Invalid(new ValidationError(NotExpense));

        await using var scope = await db.TryBeginWriteAsync(cancellationToken);
        if (scope is null) return Result<PurchaseInvoicePaymentsResponse>.Conflict(PurchaseRegisterSync.AlreadyRunning);
        var message = await db.Messages.FirstOrDefaultAsync(x => x.Id == command.MessageId, cancellationToken);
        if (message is null) return Result<PurchaseInvoicePaymentsResponse>.NotFound(ReviewPurchaseInvoiceCommandHandler.NotFound);
        if (message.ReviewVersion != command.Version)
            return Result<PurchaseInvoicePaymentsResponse>.Conflict(ReviewPurchaseInvoiceCommandHandler.Stale);
        if (await db.Payments.AnyAsync(x => x.MessageId == message.Id && x.TransactionId == command.TransactionId
                                            && x.UnlinkedAt == null, cancellationToken))
            return Result<PurchaseInvoicePaymentsResponse>.Conflict(AlreadyLinked);

        db.Payments.Add(message.LinkPayment(command.TransactionId, command.By, clock.GetUtcNow()));
        await db.SaveChangesAsync(cancellationToken);
        await scope.CommitAsync(cancellationToken);
        return Result.Success(await RespondAsync(message, cancellationToken));
    }

    public async ValueTask<Result<PurchaseInvoicePaymentsResponse>> Handle(UnlinkPurchasePaymentCommand command,
        CancellationToken cancellationToken)
    {
        await using var scope = await db.TryBeginWriteAsync(cancellationToken);
        if (scope is null) return Result<PurchaseInvoicePaymentsResponse>.Conflict(PurchaseRegisterSync.AlreadyRunning);
        var message = await db.Messages.FirstOrDefaultAsync(x => x.Id == command.MessageId, cancellationToken);
        if (message is null) return Result<PurchaseInvoicePaymentsResponse>.NotFound(ReviewPurchaseInvoiceCommandHandler.NotFound);
        if (message.ReviewVersion != command.Version)
            return Result<PurchaseInvoicePaymentsResponse>.Conflict(ReviewPurchaseInvoiceCommandHandler.Stale);
        var link = await db.Payments.FirstOrDefaultAsync(x => x.MessageId == message.Id
            && x.TransactionId == command.TransactionId && x.UnlinkedAt == null, cancellationToken);
        if (link is null) return Result<PurchaseInvoicePaymentsResponse>.Conflict(NotLinked);

        var unlinked = message.UnlinkPayment(link, command.By, clock.GetUtcNow());
        if (unlinked.IsFailure) return unlinked.ToResult<PurchaseInvoicePaymentsResponse>();
        await db.SaveChangesAsync(cancellationToken);
        await scope.CommitAsync(cancellationToken);
        return Result.Success(await RespondAsync(message, cancellationToken));
    }

    private async Task<PurchaseInvoicePaymentsResponse> RespondAsync(PurchaseMessage message,
        CancellationToken cancellationToken)
    {
        var links = (await PurchasePayments.ActiveLinksAsync(db, [message.Id], cancellationToken))[message.Id].ToList();
        var payments = PurchasePayments.Respond(links, await PurchasePayments.OperationsAsync(reader, links, cancellationToken));
        var total = PurchasePayments.TotalMinor(await PurchasePayments.DocumentsAsync(db, message.Id, cancellationToken));
        return new PurchaseInvoicePaymentsResponse(message.Id, message.ReviewVersion, total, payments,
            PurchasePayments.Issues(payments, total));
    }
}
