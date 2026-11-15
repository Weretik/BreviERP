using Purchasing.Application.Features.Documents.Queries.GetPurchaseDocumentTemplate;
using BuildingBlocks.Api.Errors;

namespace Purchasing.Api.Endpoints.Documents.GetPurchaseDocumentTemplate;

/// <summary>«Шаблон JSON»: the saved facts, the last AI answer or empty fields, with the file's UID and link.</summary>
internal static class GetPurchaseDocumentTemplateEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("files/{fileId}/document-template", HandleAsync).WithName("getPurchaseDocumentTemplate")
            .WithSummary("Get purchase document template")
            .WithDescription("«Шаблон JSON»: the saved facts, else the last AI answer, else empty fields, with sourceUID and originalFileLink filled in.")
            .Produces<object>(contentType: "application/json");

    private static async Task<IResult> HandleAsync(string fileId, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPurchaseDocumentTemplateQuery(fileId), cancellationToken);
        return result.IsSuccess ? Results.Content(result.Value, "application/json") : result.ToHttpResult();
    }
}
