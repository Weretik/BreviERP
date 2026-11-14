using Purchasing.Application.Features.Documents.Commands.RecognizePurchaseFile;
using Purchasing.Application.Features.Documents.Shared;
using Purchasing.Application.Features.Documents.Shared.Recognition;
using BuildingBlocks.Api.Errors;

namespace Purchasing.Api.Endpoints.Documents.RecognizePurchaseFile;

/// <summary>
/// «Розпізнати» / «Розпізнати знову» one file with the AI. 503 when the AI is off, has no key or reached its limit;
/// 409 while the sheet sync runs. Problems of the answer come back with 200 and show on the card.
/// </summary>
internal static class RecognizePurchaseFileEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("files/{fileId}/recognize", HandleAsync).WithName("recognizePurchaseFile")
            .Produces<PurchaseRecognitionResponse>();

    private static async Task<IResult> HandleAsync(string fileId, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RecognizePurchaseFileCommand(fileId, user.ActorName()), cancellationToken);
        return result.Status == ResultStatus.Unavailable
            ? Results.Json(new { errors = result.Errors }, statusCode: StatusCodes.Status503ServiceUnavailable)
            : result.ToHttpResult();
    }
}
