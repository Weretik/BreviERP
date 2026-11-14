using Purchasing.Application.Abstractions;
using Purchasing.Domain.Messages;

using Purchasing.Application.Features.Register.Shared;

namespace Purchasing.Application.Features.Register.Commands.UnlinkPurchasePayment;

/// <summary>«Відв'язати»: removes an active payment link; the link stays in the history.</summary>
public sealed record UnlinkPurchasePaymentCommand(string MessageId, Guid TransactionId, int Version, string By)
    : ICommand<Result<PurchaseInvoicePaymentsResponse>>;
