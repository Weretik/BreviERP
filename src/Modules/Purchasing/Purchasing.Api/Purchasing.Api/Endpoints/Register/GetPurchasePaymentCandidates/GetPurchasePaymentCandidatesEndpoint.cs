using Purchasing.Application.Features.Register.Queries.GetPurchasePaymentCandidates;
using Purchasing.Application.Features.Register.Shared;
using BuildingBlocks.Api.Errors;

namespace Purchasing.Api.Endpoints.Register.GetPurchasePaymentCandidates;

/// <summary>«Прив'язати оплату»: expenses of «Бухгалтерія» that may pay this invoice (at most 20).</summary>
internal static class GetPurchasePaymentCandidatesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{messageId}/payments/candidates", HandleAsync).WithName("getPurchasePaymentCandidates")
            .WithSummary("Expenses of «Бухгалтерія» for «Прив'язати оплату» (accounting stage 9), at most 20, active, of both scopes")
            .WithDescription("Expenses of «Бухгалтерія» for «Прив'язати оплату» (accounting stage 9), at most 20, active, of both scopes. Without search — the expenses of the linked supplier, or without a supplier those equal to the invoice total; with search — any expense whose description, note or one-off name contains it, or whose amount equals it. Already linked ones are left out; those equal to the invoice total come first (sameAmount).")
            .Produces<IReadOnlyList<PurchasePaymentCandidateResponse>>();

    private static async Task<IResult> HandleAsync(string messageId, [FromServices] ISender sender,
        string? search = null, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetPurchasePaymentCandidatesQuery(messageId, search), cancellationToken)).ToHttpResult();
}
