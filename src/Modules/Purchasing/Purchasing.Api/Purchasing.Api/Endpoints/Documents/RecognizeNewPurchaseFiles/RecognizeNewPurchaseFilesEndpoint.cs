using Purchasing.Application.Features.Documents.Commands.RecognizeNewPurchaseFiles;
using Purchasing.Application.Features.Documents.Shared;

namespace Purchasing.Api.Endpoints.Documents.RecognizeNewPurchaseFiles;

/// <summary>One batch of «Розпізнати всі нові»: repeat while <c>remaining</c> is above 0 and there is no <c>stopReason</c>.</summary>
internal static class RecognizeNewPurchaseFilesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("recognize-new", HandleAsync).WithName("recognizeNewPurchaseFiles")
            .Produces<PurchaseRecognitionBatchResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new RecognizeNewPurchaseFilesCommand(user.ActorName()), cancellationToken)).ToHttpResult();
}
