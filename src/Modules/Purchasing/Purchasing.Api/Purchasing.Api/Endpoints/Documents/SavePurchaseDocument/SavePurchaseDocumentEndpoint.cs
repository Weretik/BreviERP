using System.Text.Json;
using Purchasing.Application.Features.Documents.Commands.SaveCheckedPurchaseDocument;
using Purchasing.Application.Features.Documents.Shared;

namespace Purchasing.Api.Endpoints.Documents.SavePurchaseDocument;

/// <summary>«Завантажити JSON»: 400 with the problems when the facts fail the checks; 409 when other facts are saved and <c>replace</c> is false.</summary>
internal static class SavePurchaseDocumentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("files/{fileId}/document", HandleAsync).WithName("savePurchaseDocument")
            .Produces<PurchaseDocumentSaveResponse>();

    private static async Task<IResult> HandleAsync(string fileId, PurchaseDocumentSaveRequest request,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new SaveCheckedPurchaseDocumentCommand(fileId, request.Document, request.Replace,
            user.ActorName()), cancellationToken)).ToHttpResult();
}

/// <summary><c>Document</c> is the JSON of the facts; <c>Replace</c> confirms replacing different saved facts.</summary>
public sealed record PurchaseDocumentSaveRequest(JsonElement Document, bool Replace);
