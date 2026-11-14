using Content.Application.Features.Articles.Commands.DeleteContentArticle;

namespace Content.Api.Endpoints.Articles.DeleteContentArticle;

/// <summary>Removes the article; its photos stay in the library.</summary>
internal static class DeleteContentArticleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync).WithName("deleteContentArticle");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteContentArticleCommand(id), cancellationToken);
        return result.Status switch
        {
            ResultStatus.Ok or ResultStatus.NoContent => Results.NoContent(),
            ResultStatus.NotFound => Results.NotFound(),
            ResultStatus.Invalid => Results.BadRequest(result.ValidationErrors),
            _ => Results.StatusCode(StatusCodes.Status500InternalServerError),
        };
    }
}
