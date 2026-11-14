using Purchasing.Application.Features.Register.Commands.LinkPurchasePayment;
using Purchasing.Application.Features.Register.Shared;

namespace Purchasing.Api.Endpoints.Register.Shared;

/// <summary><c>Version</c> is the <c>reviewVersion</c> the user saw.</summary>
public sealed record PurchasePaymentRequest(Guid TransactionId, int Version);
