using Reference.Application.Features.ProductSection.Commands.UpdateProductSection;

namespace Reference.Api.Endpoints.ProductSections;

internal static class UpdateProductSectionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync)
            .RequireAuthorization("access:product-sections");

    private static async Task<IResult> HandleAsync(int id, UpdateProductSectionRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateProductSectionCommand(id, new UpdateProductSectionCommandRequest(request.Type,
            request.Name,
            request.IsActive, request.Comment, request.SortOrder)), cancellationToken)).ToHttpResult();
}

/// <summary>Editable fields of a product section; <paramref name="Type"/> is <c>Sewing</c> or <c>Ppe</c>.</summary>
public sealed record UpdateProductSectionRequest(
    string Type,
    string Name,
    bool IsActive = true,
    string? Comment = null,
    int SortOrder = 0);
