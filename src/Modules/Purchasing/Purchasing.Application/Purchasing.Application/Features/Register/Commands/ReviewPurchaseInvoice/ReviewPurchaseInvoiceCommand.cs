using Purchasing.Application.Abstractions;
using Purchasing.Domain.Messages;
using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;

namespace Purchasing.Application.Features.Register.Commands.ReviewPurchaseInvoice;

/// <summary>
/// «Постачальник і примітка» of one card: links an active supplier (or none) and writes a note. <paramref name="Version"/>
/// is the review version the user saw; a stale one is refused so a newer change is never overwritten.
/// </summary>
public sealed record ReviewPurchaseInvoiceCommand(string MessageId, int? SupplierId, string? Note, int Version, string Reviewer)
    : ICommand<Result<PurchaseInvoiceReviewResponse>>;
