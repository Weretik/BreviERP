using Purchasing.Application.Features.Documents.Commands.StartNewPurchaseFilesRecognition;
using BuildingBlocks.Api.Errors;

namespace Purchasing.Api.Endpoints.Documents.RecognizeNewPurchaseFiles;

/// <summary>
/// «Розпізнати всі нові»: starts the «Розпізнавання оригіналів закупки» background task instead of reading the files
/// inside the request. 409 when it already runs or waits to start.
/// </summary>
internal static class RecognizeNewPurchaseFilesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("recognize-new", HandleAsync).WithName("startNewPurchaseFilesRecognition")
            .WithSummary("Start the background recognition of new purchase originals")
            .WithDescription("«Розпізнати всі нові»: counts the active safe originals never tried before and, when there are any, asks «Фонові задачі» to start the «Розпізнавання оригіналів закупки» task; the next scheduler tick, within a minute, runs it in the tasks job. started is false when nothing waits. 409 when the task already runs or waits to start. The result shows on the «Фонові задачі» page and the recognised documents in the list.")
            .Produces<PurchaseRecognitionStartResponse>()
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new StartNewPurchaseFilesRecognitionCommand(user.ActorName()), cancellationToken)).ToHttpResult();
}
