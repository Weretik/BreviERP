using Purchasing.Application.Abstractions;
using Purchasing.Application.Contracts;
using Purchasing.Application.Register.Sync;
using Purchasing.Domain.Messages;

namespace Purchasing.Application.Register.Review;

/// <summary>
/// «Постачальник і примітка» of one card: links an active supplier (or none) and writes a note. <paramref name="Version"/>
/// is the review version the user saw; a stale one is refused so a newer change is never overwritten.
/// </summary>
public sealed record ReviewPurchaseInvoiceCommand(string MessageId, int? SupplierId, string? Note, int Version, string Reviewer)
    : ICommand<Result<PurchaseInvoiceReviewResponse>>;

public sealed class ReviewPurchaseInvoiceCommandValidator : AbstractValidator<ReviewPurchaseInvoiceCommand>
{
    public ReviewPurchaseInvoiceCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.SupplierId).GreaterThan(0).When(x => x.SupplierId is not null);
        RuleFor(x => x.Note).Must(note => note is null || note.Trim().Length <= PurchaseMessage.ReviewNoteMaxLength)
            .WithMessage($"Примітка — не більше {PurchaseMessage.ReviewNoteMaxLength} символів.");
        RuleFor(x => x.Version).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Reviewer).NotEmpty().MaximumLength(PurchaseMessage.ReviewerMaxLength);
    }
}

public sealed class ReviewPurchaseInvoiceCommandHandler(
    IPurchasingDbContext db,
    IPurchasingSupplierReader suppliers,
    TimeProvider clock) : ICommandHandler<ReviewPurchaseInvoiceCommand, Result<PurchaseInvoiceReviewResponse>>
{
    public const string NotFound = "Рахунок не знайдено. Оновіть реєстр.";
    public const string SupplierNotFound = "Постачальника не знайдено або він не використовується.";
    public const string Stale = "Цей рахунок уже змінили. Відкрийте його знову, щоб побачити нові дані.";

    public async ValueTask<Result<PurchaseInvoiceReviewResponse>> Handle(ReviewPurchaseInvoiceCommand command,
        CancellationToken cancellationToken)
    {
        PurchasingSupplier? supplier = null;
        if (command.SupplierId is { } supplierId)
        {
            supplier = (await suppliers.ListAsync(cancellationToken)).FirstOrDefault(x => x.Id == supplierId);
            if (supplier is null) return Result<PurchaseInvoiceReviewResponse>.NotFound(SupplierNotFound);
        }

        await using var scope = await db.TryBeginWriteAsync(cancellationToken);
        if (scope is null) return Result<PurchaseInvoiceReviewResponse>.Conflict(PurchaseRegisterSync.AlreadyRunning);

        var message = await db.Messages.FirstOrDefaultAsync(x => x.Id == command.MessageId, cancellationToken);
        if (message is null) return Result<PurchaseInvoiceReviewResponse>.NotFound(NotFound);
        if (message.ReviewVersion != command.Version) return Result<PurchaseInvoiceReviewResponse>.Conflict(Stale);
        // An inactive supplier may stay linked, but a new link needs an active one.
        if (supplier is { Active: false } && supplier.Id != message.SupplierId)
            return Result<PurchaseInvoiceReviewResponse>.NotFound(SupplierNotFound);

        var change = message.Review(command.SupplierId, command.Note, command.Reviewer, clock.GetUtcNow());
        if (change is not null)
        {
            db.ReviewChanges.Add(change);
            await db.SaveChangesAsync(cancellationToken);
            await scope.CommitAsync(cancellationToken);
        }

        return Result.Success(new PurchaseInvoiceReviewResponse(message.Id, message.SupplierId, supplier?.Name,
            message.ReviewNote, message.ReviewVersion, message.ReviewedBy, message.ReviewedAt));
    }
}
