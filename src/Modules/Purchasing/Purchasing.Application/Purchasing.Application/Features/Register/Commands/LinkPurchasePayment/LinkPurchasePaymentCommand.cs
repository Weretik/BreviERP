using Purchasing.Application.Abstractions;
using Purchasing.Domain.Messages;

using Purchasing.Application.Features.Register.Shared;

namespace Purchasing.Application.Features.Register.Commands.LinkPurchasePayment;

/// <summary>
/// «Прив'язати оплату»: links an active expense of «Бухгалтерія» to the invoice. <paramref name="Version"/> is the review
/// version the user saw; the same operation may pay several invoices.
/// </summary>
public sealed record LinkPurchasePaymentCommand(string MessageId, Guid TransactionId, int Version, string By)
    : ICommand<Result<PurchaseInvoicePaymentsResponse>>;
