using Purchasing.Application.Features.Register.Commands.UnlinkPurchasePayment;
using Purchasing.Application.Features.Register.Shared;
using Purchasing.Api.Endpoints.Register.Shared;
using BuildingBlocks.Api.Errors;

namespace Purchasing.Api.Endpoints.Register.UnlinkPurchasePayment;

/// <summary>Removes a payment link (it stays in the history): 409 when the version is stale or it is already removed.</summary>
internal static class UnlinkPurchasePaymentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{messageId}/payments/unlink", HandleAsync).WithName("unlinkPurchasePayment")
            .Produces<PurchaseInvoicePaymentsResponse>();

    private static async Task<IResult> HandleAsync(string messageId, PurchasePaymentRequest request,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UnlinkPurchasePaymentCommand(messageId, request.TransactionId, request.Version,
            user.ActorName()), cancellationToken)).ToHttpResult();
}
