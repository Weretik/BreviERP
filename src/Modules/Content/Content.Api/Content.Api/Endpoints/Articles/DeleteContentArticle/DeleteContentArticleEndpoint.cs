using BuildingBlocks.Api.Errors;
using Content.Application.Features.Articles.Commands.DeleteContentArticle;

namespace Content.Api.Endpoints.Articles.DeleteContentArticle;

/// <summary>Removes the article; its photos stay in the library.</summary>
internal static class DeleteContentArticleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync).WithName("deleteContentArticle")
            .WithSummary("Removes the article with its texts")
            .WithDescription("Removes the article with its texts. Its photos stay in the «Фото» library.");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteContentArticleCommand(id), cancellationToken);
        return result.ToNoContentResult();
    }
}
