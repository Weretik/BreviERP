using Production.Application.Features.Orders.Commands.UploadProductionOrderFile;

namespace Production.Api.Endpoints.Orders.UploadProductionOrderFile;

/// <summary>Uploads a file of the order or of one item to the CDN, in the order's folder.</summary>
internal static class UploadProductionOrderFileEndpoint
{
    /// <summary>20 MB file and the form fields around it.</summary>
    private const long MaxRequestBytes = 21 * 1024 * 1024;

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:int}/files", HandleAsync).WithName("uploadProductionOrderFile")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBytes))
            .WithMetadata(new RequestFormLimitsAttribute { MultipartBodyLengthLimit = MaxRequestBytes });

    /// <param name="kind">Sizes, Application, Extra or Photo.</param>
    /// <param name="itemId">The item the file is for; empty for the whole order.</param>
    private static async Task<IResult> HandleAsync(int id, IFormFile? file, [FromForm] string kind,
        [FromForm] int? itemId, ClaimsPrincipal user, [FromServices] ISender sender,
            CancellationToken cancellationToken)
    {
        if (user.Actor() is not { } actor) return Results.Forbid();
        if (file is null) return Results.BadRequest(new { error = "Оберіть файл." });
        await using var content = file.OpenReadStream();
        return (await sender.Send(new UploadProductionOrderFileCommand(actor, id, itemId, kind, content, file.FileName,
            file.ContentType, file.Length), cancellationToken)).ToHttpResult();
    }
}
