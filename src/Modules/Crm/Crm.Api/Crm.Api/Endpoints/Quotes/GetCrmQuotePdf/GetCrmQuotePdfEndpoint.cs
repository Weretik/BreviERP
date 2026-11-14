using Crm.Api.Access;
using Crm.Application.Features.Quotes.Queries.GetQuotePdf;

namespace Crm.Api.Endpoints.Quotes.GetCrmQuotePdf;

/// <summary>«Сформувати КП»: the PDF of the saved КП, only what the client may see (R-14).</summary>
internal static class GetCrmQuotePdfEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:long}/pdf", HandleAsync).WithName("getCrmQuotePdf");

    private static async Task<IResult> HandleAsync(long id, ClaimsPrincipal user, HttpResponse response,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        var result = await sender.Send(new GetQuotePdfQuery(actor, id), cancellationToken);
        if (!result.IsSuccess) return result.ToHttpResult();
        response.Headers.CacheControl = "no-store";
        return Results.File(result.Value.Content, "application/pdf", result.Value.FileName);
    }
}
