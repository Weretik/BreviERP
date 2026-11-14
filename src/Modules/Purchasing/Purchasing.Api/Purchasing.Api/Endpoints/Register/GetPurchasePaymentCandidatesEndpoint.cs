using Purchasing.Application.Features.Register.Queries.GetPurchasePaymentCandidates;
using Purchasing.Application.Features.Register.Shared;

namespace Purchasing.Api.Endpoints.Register;

/// <summary>«Прив'язати оплату»: expenses of «Бухгалтерія» that may pay this invoice (at most 20).</summary>
internal static class GetPurchasePaymentCandidatesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{messageId}/payments/candidates", HandleAsync).WithName("getPurchasePaymentCandidates")
            .Produces<IReadOnlyList<PurchasePaymentCandidateResponse>>();

    private static async Task<IResult> HandleAsync(string messageId, [FromServices] ISender sender,
        string? search = null, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetPurchasePaymentCandidatesQuery(messageId, search), cancellationToken)).ToHttpResult();
}
