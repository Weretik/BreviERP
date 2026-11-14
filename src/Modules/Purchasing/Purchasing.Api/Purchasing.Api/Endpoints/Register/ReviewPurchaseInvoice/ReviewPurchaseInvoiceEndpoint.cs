using Purchasing.Application.Features.Register.Commands.ReviewPurchaseInvoice;
using Purchasing.Application.Features.Register.Shared;
using BuildingBlocks.Api.Errors;

namespace Purchasing.Api.Endpoints.Register.ReviewPurchaseInvoice;

/// <summary>
/// «Постачальник і примітка»: 409 when the version is stale or a sync is running, 404 when the card or the supplier
/// is not found.
/// </summary>
internal static class ReviewPurchaseInvoiceEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{messageId}/review", HandleAsync).WithName("reviewPurchaseInvoice")
            .Produces<PurchaseInvoiceReviewResponse>();

    private static async Task<IResult> HandleAsync(string messageId, PurchaseInvoiceReviewRequest request,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ReviewPurchaseInvoiceCommand(messageId, request.SupplierId, request.Note,
            request.Version, user.ActorName()), cancellationToken)).ToHttpResult();
}

/// <summary><c>Version</c> is the <c>reviewVersion</c> the user saw; <c>SupplierId</c> null unlinks.</summary>
public sealed record PurchaseInvoiceReviewRequest(int? SupplierId, string? Note, int Version);
