using Purchasing.Application.Features.Register.Commands.LinkPurchasePayment;
using Purchasing.Application.Features.Register.Shared;
using Purchasing.Api.Endpoints.Register.Shared;
using BuildingBlocks.Api.Errors;

namespace Purchasing.Api.Endpoints.Register.LinkPurchasePayment;

/// <summary>
/// Links an active expense as a payment: 400 when it is not one, 409 when the version is stale, it is already linked
/// or a sync is running.
/// </summary>
internal static class LinkPurchasePaymentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{messageId}/payments", HandleAsync).WithName("linkPurchasePayment")
            .WithSummary("Links an active expense of «Бухгалтерія» as a payment of the invoice and adds 1 to reviewVersion")
            .WithDescription("Links an active expense of «Бухгалтерія» as a payment of the invoice and adds 1 to reviewVersion. The same operation may pay several invoices; nothing is booked in «Бухгалтерія».")
            .Produces<PurchaseInvoicePaymentsResponse>();

    private static async Task<IResult> HandleAsync(string messageId, PurchasePaymentRequest request,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new LinkPurchasePaymentCommand(messageId, request.TransactionId, request.Version,
            user.ActorName()), cancellationToken)).ToHttpResult();
}
